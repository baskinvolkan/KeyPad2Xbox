using System.Runtime.InteropServices;

namespace KeyPad2Xbox.Core.Native
{
    // Layout: InterceptionKeyStroke is 8 bytes (code+state+information). The native
    // InterceptionStroke buffer is sized to InterceptionMouseStroke (~20 bytes), but
    // interception_receive/send only touches the KeyStroke prefix when the device is
    // a keyboard. Size = 16 leaves headroom and matches the previously verified ABI.
    // WARNING: do NOT reuse this struct for mouse devices — Size=16 is smaller than
    // InterceptionMouseStroke and would cause a buffer overflow.
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct Stroke
    {
        public ushort code;
        public ushort state;
        public uint information;
    }
}
