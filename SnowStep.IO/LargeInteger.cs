using System.Runtime.InteropServices;

namespace SnowStep.IO
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LargeInteger
    {
        public readonly int Low;
        public readonly int High;

        public long ToInt64() => (this.High * 0x100000000) + this.Low;
    }
}
