using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SnowStep.IO
{
    internal sealed class BackupStream : IDisposable
    {
        #region native methods

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BackupRead(SafeFileHandle handle, ref Win32FileStreamHeader buf, int cb, out int cbRead, [MarshalAs(UnmanagedType.Bool)] bool abort, [MarshalAs(UnmanagedType.Bool)] bool processSecurity, ref IntPtr context);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BackupRead(SafeFileHandle handle, SafeHGlobalHandle buf, int cb, out int cbRead, [MarshalAs(UnmanagedType.Bool)] bool abort, [MarshalAs(UnmanagedType.Bool)] bool processSecurity, ref IntPtr context);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BackupSeek(SafeFileHandle handle, int low, int high, out int cbLow, out int cbHigh, ref IntPtr context);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string fileName, NativeFileAccess access, FileShare share, IntPtr security, FileMode mode, NativeFileFlags flags, IntPtr template);

        #endregion

        #region private fields

        private IntPtr context = IntPtr.Zero;
        private readonly SafeFileHandle fileHandle;

        #endregion

        private BackupStream(SafeFileHandle fileHandle) => this.fileHandle = fileHandle;

        public BackupStream(string filePath) : this(CreateFile(filePath, NativeFileAccess.GenericRead, FileShare.Read, IntPtr.Zero, FileMode.Open, NativeFileFlags.BackupSemantics, IntPtr.Zero)) { }

        public bool Read(ref Win32FileStreamHeader header)
        {
            var cbHeader = Marshal.SizeOf(typeof(Win32FileStreamHeader));
            return BackupRead(this.fileHandle, ref header, cbHeader, out var cbRead, false, false, ref this.context) && cbRead == cbHeader;
        }

        public bool Read(in Win32FileStreamHeader header, StreamName streamName)
        {
            streamName.EnsureCapacity(header.NameSize);
            return BackupRead(this.fileHandle, streamName, header.NameSize, out var cbRead, false, false, ref this.context) && cbRead == header.NameSize;
        }

        public bool Seek(in Win32FileStreamHeader header) => BackupSeek(this.fileHandle, header.Size.Low, header.Size.High, out var low, out var high, ref this.context);

        #region IDisposable stuff

        public void Dispose()
        {
            using (var handle = SafeHGlobalHandle.Invalid())
                BackupRead(this.fileHandle, handle, 0, out var cb, true, false, ref this.context);
            this.fileHandle.Dispose();
        }

        #endregion
    }
}
