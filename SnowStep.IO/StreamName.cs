using System;
using System.Runtime.InteropServices;

namespace SnowStep.IO
{
    internal sealed class StreamName : IDisposable
    {
        public static implicit operator SafeHGlobalHandle(StreamName name) => name.buffer;

        private static readonly SafeHGlobalHandle Invalid = SafeHGlobalHandle.Invalid();

        private SafeHGlobalHandle buffer = StreamName.Invalid;

        public StreamName()
        {
        }

        public void EnsureCapacity(int capacity)
        {
            var currentSize = this.buffer.IsInvalid ? 0 : this.buffer.Size;
            if (currentSize < capacity)
            {
                if (currentSize != 0)
                    currentSize <<= 1;
                if (currentSize < capacity)
                    currentSize = capacity;
                if (!this.buffer.IsInvalid)
                    this.buffer.Dispose();
                this.buffer = SafeHGlobalHandle.Allocate(currentSize);
            }
        }

        public string ReadStreamName(int length)
        {
            var name = ReadString(length);
            if (string.IsNullOrEmpty(name))
                return null;
            var separatorIndex = name.IndexOf(SafeNativeMethods.StreamSeparator, 1);
            if (separatorIndex == -1)
            {
                separatorIndex = name.IndexOf('\0');
                if (separatorIndex <= 1)
                    return null;
            }
            return name.Substring(1, separatorIndex - 1);
        }

        public string ReadString(int length)
        {
            if (length <= 0 || this.buffer.IsInvalid)
                return null;
            if (this.buffer.Size < length)
                length = this.buffer.Size;
            return Marshal.PtrToStringUni(this.buffer.DangerousGetHandle(), length);
        }

        #region IDisposable stuff

        public void Dispose()
        {
            if (!this.buffer.IsInvalid)
            {
                this.buffer.Dispose();
                this.buffer = StreamName.Invalid;
            }
        }

        #endregion
    }
}
