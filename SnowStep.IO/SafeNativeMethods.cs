using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace SnowStep.IO
{
    internal static class SafeNativeMethods
    {
        #region constants

        private static readonly char[] InvalidStreamNameChars = Path.GetInvalidFileNameChars().Where(c => c < 1 || 31 < c).ToArray();

        private const int ErrorFileNotFound = 2;
        private const string LongPathPrefix = @"\\?\";

        public const int DefaultBufferSize = 0x1000;
        public const int MaxPath = 256;
        public const char StreamSeparator = ':';

        #endregion

        #region native methods

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int FormatMessage(int flags, IntPtr source, int messageId, int langId, StringBuilder buffer, int size, IntPtr arguments);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetFileAttributes(string fileName);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileSizeEx(SafeFileHandle handle, out LargeInteger size);

        [DllImport("kernel32")]
        private static extern int GetFileType(SafeFileHandle handle);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string name, NativeFileAccess access, FileShare share, IntPtr security, FileMode mode, NativeFileFlags flags, IntPtr template);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        #endregion

        private static int MakeHRFromErrorCode(int errorCode) => (-2147024896 | errorCode);

        private static string GetErrorMessage(int errorCode)
        {
            var buffer = new StringBuilder(512);
            if (FormatMessage(0x3200, IntPtr.Zero, errorCode, 0, buffer, buffer.Capacity, IntPtr.Zero) == 0)
                return "Unknown Error";
            return buffer.ToString();
        }

        private static void ThrowIOError(int errorCode, string path)
        {
            switch (errorCode)
            {
                case 2:
                    if (string.IsNullOrEmpty(path))
                        throw new FileNotFoundException();
                    throw new FileNotFoundException(null, path);
                case 3:
                    if (string.IsNullOrEmpty(path))
                        throw new DirectoryNotFoundException();
                    throw new DirectoryNotFoundException(null, new FileNotFoundException(null, path));
                case 5:
                    if (string.IsNullOrEmpty(path))
                        throw new UnauthorizedAccessException();
                    throw new UnauthorizedAccessException(path);
                default:
                    Marshal.ThrowExceptionForHR(MakeHRFromErrorCode(errorCode));
                    break;
            }
        }

        public static void ThrowLastIOError(string path)
        {
            var errorCode = Marshal.GetLastWin32Error();
            if (errorCode == 0)
                return;
            var hr = Marshal.GetHRForLastWin32Error();
            if (0 <= hr)
                throw new Win32Exception(errorCode);
            ThrowIOError(errorCode, path);
        }

        public static NativeFileAccess ToNative(this FileAccess access)
        {
            NativeFileAccess result = 0;
            if ((access & FileAccess.Read) == FileAccess.Read)
                result |= NativeFileAccess.GenericRead;
            if ((access & FileAccess.Write) == FileAccess.Write)
                result |= NativeFileAccess.GenericWrite;
            return result;
        }

        public static string BuildStreamPath(string filePath, string streamName)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;
            var result = filePath;
            if (result.Length == 1)
                result = @".\" + result;
            result += SafeNativeMethods.StreamSeparator + streamName + SafeNativeMethods.StreamSeparator + "$DATA";
            if (SafeNativeMethods.MaxPath <= result.Length)
                result = SafeNativeMethods.LongPathPrefix + result;
            return result;
        }

        public static void ValidateStreamName(string streamName)
        {
            if (!string.IsNullOrEmpty(streamName) && streamName.IndexOfAny(SafeNativeMethods.InvalidStreamNameChars) != -1)
                throw new ArgumentException("Invalid file char");
        }

        public static int SafeGetFileAttributes(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            var result = GetFileAttributes(name);
            if (result == -1)
            {
                var errorCode = Marshal.GetLastWin32Error();
                if (errorCode != ErrorFileNotFound)
                    ThrowLastIOError(name);
            }
            return result;
        }

        public static bool SafeDeleteFile(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            var result = DeleteFile(name);
            if (!result)
            {
                var errorCode = Marshal.GetLastWin32Error();
                if (errorCode != ErrorFileNotFound)
                    ThrowLastIOError(name);
            }
            return result;
        }

        public static SafeFileHandle SafeCreateFile(string path, NativeFileAccess access, FileShare share, IntPtr security, FileMode mode, NativeFileFlags flags, IntPtr template)
        {
            var result = CreateFile(path, access, share, security, mode, flags, template);
            if (!result.IsInvalid && GetFileType(result) != 1)
            {
                result.Dispose();
                throw new NotSupportedException();
            }
            return result;
        }

        public static long GetFileSize(string path, SafeFileHandle handle)
        {
            var result = 0L;
            if (handle != null && !handle.IsInvalid)
            {
                var value = default(LargeInteger);
                if (GetFileSizeEx(handle, out value))
                    result = value.ToInt64();
                else
                    ThrowLastIOError(path);
            }
            return result;
        }

        public static long GetFileSize(string path)
        {
            if (!string.IsNullOrEmpty(path))
                using (var file = SafeCreateFile(path, NativeFileAccess.GenericRead, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
                    return GetFileSize(path, file);
            return 0;
        }

        public static IEnumerable<Win32StreamInfo> ListStreams(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (filePath.IndexOfAny(SafeNativeMethods.InvalidStreamNameChars) != -1)
                throw new ArgumentException(nameof(filePath));
            using (var backupStream = new BackupStream(filePath))
            using (var hName = new StreamName())
            {
                var header = default(Win32FileStreamHeader);
                while (backupStream.Read(ref header))
                {
                    if (!backupStream.Read(in header, hName))
                        break;
                    var name = hName.ReadStreamName(header.NameSize >> 1);
                    if (!string.IsNullOrEmpty(name))
                        yield return new Win32StreamInfo
                        {
                            StreamType = (FileStreamType)header.Id,
                            StreamAttributes = (FileStreamAttributes)header.Attributes,
                            StreamSize = header.Size.ToInt64(),
                            StreamName = name,
                        };
                    if (!backupStream.Seek(in header))
                        break;
                }
            }
        }
    }
}
