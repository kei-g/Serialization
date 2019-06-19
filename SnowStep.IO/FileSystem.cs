using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;

namespace SnowStep.IO
{
    public static class FileSystem
    {
        private static FileSystemInfo CreateInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            path = Path.GetFullPath(path);
            if (!File.Exists(path) && Directory.Exists(path))
                return new DirectoryInfo(path);
            return new FileInfo(path);
        }

        public static IEnumerable<AlternateDataStreamInfo> ListAlternateDataStreams(this FileSystemInfo file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (!file.Exists)
                throw new FileNotFoundException(null, file.FullName);
            var path = file.FullName;
            new FileIOPermission(FileIOPermissionAccess.Read, path).Demand();
            return SafeNativeMethods.ListStreams(path).Select(info => new AlternateDataStreamInfo(path, info));
        }

        public static IEnumerable<AlternateDataStreamInfo> ListAlternateDataStreams(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            return CreateInfo(filePath).ListAlternateDataStreams();
        }

        public static bool AlternateDataStreamExists(this FileSystemInfo file, string streamName)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            SafeNativeMethods.ValidateStreamName(streamName);
            var path = SafeNativeMethods.BuildStreamPath(file.FullName, streamName);
            return SafeNativeMethods.SafeGetFileAttributes(path) != -1;
        }

        public static bool AlternateDataStreamExists(string filePath, string streamName)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            return CreateInfo(filePath).AlternateDataStreamExists(streamName);
        }

        public static AlternateDataStreamInfo GetAlternateDataStream(this FileSystemInfo file, string streamName, FileMode mode = FileMode.OpenOrCreate)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (!file.Exists)
                throw new FileNotFoundException(null, file.FullName);
            SafeNativeMethods.ValidateStreamName(streamName);
            if (mode == FileMode.Truncate || mode == FileMode.Append)
                throw new NotSupportedException();
            new FileIOPermission(mode == FileMode.Open ? FileIOPermissionAccess.Read : FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, file.FullName).Demand();
            var path = SafeNativeMethods.BuildStreamPath(file.FullName, streamName);
            var exists = SafeNativeMethods.SafeGetFileAttributes(path) != -1;
            if (exists && mode == FileMode.Create)
                throw new IOException("Stream already exists");
            if (!exists && mode == FileMode.Open)
                throw new IOException("Stream doesnot exist");
            return new AlternateDataStreamInfo(file.FullName, streamName, path, exists);
        }

        public static AlternateDataStreamInfo GetAlternateDataStream(string filePath, string streamName, FileMode mode = FileMode.OpenOrCreate)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            return CreateInfo(filePath).GetAlternateDataStream(streamName, mode);
        }

        public static bool DeleteAlternateDataStream(this FileSystemInfo file, string streamName)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            SafeNativeMethods.ValidateStreamName(streamName);
            new FileIOPermission(FileIOPermissionAccess.Write, file.FullName).Demand();
            if (!file.Exists)
                return false;
            var path = SafeNativeMethods.BuildStreamPath(file.FullName, streamName);
            if (SafeNativeMethods.SafeGetFileAttributes(path) == -1)
                return false;
            return SafeNativeMethods.SafeDeleteFile(path);
        }

        public static bool DeleteAlternateDataStream(string filePath, string streamName)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            return CreateInfo(filePath).DeleteAlternateDataStream(streamName);
        }
    }
}
