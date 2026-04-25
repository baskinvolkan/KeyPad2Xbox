using System.Runtime.InteropServices;

namespace KeyPad2Xbox.Core.Native
{
    // The structure needs to be large enough to hold either KeyStroke or MouseStroke.
    // StructLayout Size = 16 is sufficient for this union.
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct Stroke
    {
        public ushort code;
        public ushort state;
        public uint information;
    }
}
