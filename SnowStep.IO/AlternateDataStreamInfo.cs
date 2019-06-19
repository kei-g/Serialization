using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;

namespace SnowStep.IO
{
    [DebuggerDisplay("{FullPath}")]
    public sealed class AlternateDataStreamInfo : IEquatable<AlternateDataStreamInfo>
    {
        #region private fields

        private readonly FileStreamAttributes attributes;
        private readonly bool exists;
        private readonly string filePath;
        private readonly string fullPath;
        private readonly long size;
        private readonly string streamName;
        private readonly FileStreamType streamType;

        #endregion

        #region constructors

        internal AlternateDataStreamInfo(string filePath, Win32StreamInfo info)
        {
            this.attributes = info.StreamAttributes;
            this.exists = true;
            this.filePath = filePath;
            this.fullPath = SafeNativeMethods.BuildStreamPath(filePath, info.StreamName);
            this.size = info.StreamSize;
            this.streamName = info.StreamName;
            this.streamType = info.StreamType;
        }

        internal AlternateDataStreamInfo(string filePath, string streamName, string fullPath, bool exists)
        {
            if (string.IsNullOrEmpty(fullPath))
                fullPath = SafeNativeMethods.BuildStreamPath(filePath, streamName);
            this.exists = exists;
            this.filePath = filePath;
            this.fullPath = fullPath;
            if (exists)
                this.size = SafeNativeMethods.GetFileSize(this.fullPath);
            this.streamName = streamName;
            this.streamType = FileStreamType.AlternateDataStream;
        }

        #endregion

        #region properties

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FileStreamAttributes Attributes { get => this.attributes; }

        public bool Exists { get => this.exists; }
        public string FilePath { get => this.filePath; }
        public string FullPath { get => this.fullPath; }
        public string Name { get => this.streamName; }
        public long Size { get => this.size; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FileStreamType StreamType { get => this.streamType; }

        #endregion

        #region System stuffs

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            var other = obj as AlternateDataStreamInfo;
            if (object.ReferenceEquals(null, other))
                return false;
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.GetHashCode(this.filePath ?? String.Empty) ^ comparer.GetHashCode(this.streamName ?? String.Empty);
        }

        public override string ToString() => this.FullPath;

        #endregion

        #region IEquatable<AlternateDataStreamInfo> stuffs

        public bool Equals(AlternateDataStreamInfo other)
        {
            if (object.ReferenceEquals(null, other))
                return false;
            if (object.ReferenceEquals(this, other))
                return true;
            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Equals(this.filePath ?? String.Empty, other.filePath ?? String.Empty)
                && comparer.Equals(this.streamName ?? String.Empty, other.streamName ?? String.Empty);
        }

        #endregion

        #region operator overloads

        public static bool operator ==(AlternateDataStreamInfo first, AlternateDataStreamInfo second)
        {
            if (object.ReferenceEquals(first, second))
                return true;
            if (object.ReferenceEquals(null, first))
                return false;
            if (object.ReferenceEquals(null, second))
                return false;
            return first.Equals(second);
        }

        public static bool operator !=(AlternateDataStreamInfo first, AlternateDataStreamInfo second)
        {
            if (object.ReferenceEquals(first, second))
                return false;
            if (object.ReferenceEquals(null, first))
                return true;
            if (object.ReferenceEquals(null, second))
                return true;
            return !first.Equals(second);
        }

        #endregion

        public bool Delete()
        {
            new FileIOPermission(FileIOPermissionAccess.Write, this.filePath).Demand();
            return SafeNativeMethods.SafeDeleteFile(this.fullPath);
        }

        private static FileIOPermissionAccess CalculateAccess(FileMode mode, FileAccess access)
        {
            FileIOPermissionAccess permission = FileIOPermissionAccess.NoAccess;
            switch (mode)
            {
                case FileMode.Append:
                    permission = FileIOPermissionAccess.Append;
                    break;
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                    permission = FileIOPermissionAccess.Write;
                    break;
                case FileMode.Open:
                    permission = FileIOPermissionAccess.Read;
                    break;
            }
            switch (access)
            {
                case FileAccess.ReadWrite:
                    permission |= FileIOPermissionAccess.Read;
                    permission |= FileIOPermissionAccess.Write;
                    break;
                case FileAccess.Read:
                    permission |= FileIOPermissionAccess.Read;
                    break;
                case FileAccess.Write:
                    permission |= FileIOPermissionAccess.Write;
                    break;
            }
            return permission;
        }

        public FileStream Open(FileMode mode, FileAccess access, FileShare share = FileShare.None, int bufferSize = SafeNativeMethods.DefaultBufferSize, bool useAsync = false)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, null);
            new FileIOPermission(CalculateAccess(mode, access), this.filePath).Demand();
            var flags = useAsync ? NativeFileFlags.Overlapped : 0;
            var handle = SafeNativeMethods.SafeCreateFile(this.FullPath, access.ToNative(), share, IntPtr.Zero, mode, flags, IntPtr.Zero);
            if (handle.IsInvalid)
                SafeNativeMethods.ThrowLastIOError(this.FullPath);
            return new FileStream(handle, access, bufferSize, useAsync);
        }

        public FileStream Open(FileMode mode) => Open(mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite);

        public FileStream OpenRead() => Open(FileMode.Open, FileAccess.Read, FileShare.Read);

        public FileStream OpenWrite() => Open(FileMode.OpenOrCreate, FileAccess.Write);

        public StreamReader OpenText() => new StreamReader(OpenRead());
    }
}
