using System.Runtime.InteropServices;

namespace SnowStep.IO
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32FileStreamHeader
    {
        public readonly int Id;
        public readonly int Attributes;
        public readonly LargeInteger Size;
        public readonly int NameSize;
    }
}
