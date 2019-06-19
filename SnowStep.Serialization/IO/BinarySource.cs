using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SnowStep.Serialization.IO
{
    public class BinarySource
    {
        private readonly byte[] buf;

        private long position;

        private readonly byte[] temp = new byte[8];

        public BinarySource(byte[] buf) => this.buf = buf;

        public long Read(byte[] buffer, long offset, long count)
        {
            var length = Math.Min(this.buf.LongLength - this.position, count);
            Array.Copy(this.buf, this.position, buffer, offset, length);
            this.position += length;
            return length;
        }

        public byte ReadByte()
        {
            if (Read(this.temp, 0, sizeof(byte)) < sizeof(byte))
                throw new EndOfStreamException();
            return this.temp[0];
        }

        public int ReadInt32()
        {
            if (Read(this.temp, 0, sizeof(int)) < sizeof(int))
                throw new EndOfStreamException();
            return BitConverter.ToInt32(this.temp, 0);
        }

        public long ReadInt64()
        {
            if (Read(this.temp, 0, sizeof(long)) < sizeof(long))
                throw new EndOfStreamException();
            return BitConverter.ToInt64(this.temp, 0);
        }

        public string ReadString(Encoding encoding)
        {
            var temp = new List<byte>();
            while (true)
            {
                var c = ReadByte();
                if (c == 0)
                    break;
                temp.Add(c);
            }
            return encoding.GetString(temp.ToArray());
        }

        public uint ReadUInt32()
        {
            if (Read(this.temp, 0, sizeof(uint)) < sizeof(uint))
                throw new EndOfStreamException();
            return BitConverter.ToUInt32(this.temp, 0);
        }
        public ulong ReadUInt64()
        {
            if (Read(this.temp, 0, sizeof(ulong)) < sizeof(ulong))
                throw new EndOfStreamException();
            return BitConverter.ToUInt64(this.temp, 0);
        }
    }
}
