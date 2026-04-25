using System;
using System.Runtime.InteropServices;

namespace KeyPad2Xbox.Core.Native
{
    public static class InterceptionNative
    {
        private const string DllName = "interception.dll";

        // Filter flags
        public const ushort FILTER_KEY_ALL = 0xFFFF;
        public const ushort FILTER_KEY_NONE = 0x0000;

        // Constants for Devices
        public const int MAX_KEYBOARD = 10;
        public const int MAX_MOUSE = 10;
        public const int MAX_DEVICE = MAX_KEYBOARD + MAX_MOUSE;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Predicate(int device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr interception_create_context();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void interception_destroy_context(IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void interception_set_filter(IntPtr context, Predicate predicate, ushort filter);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_wait(IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_receive(IntPtr context, int device, ref Stroke stroke, uint nstroke);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_send(IntPtr context, int device, ref Stroke stroke, uint nstroke);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_get_hardware_id(IntPtr context, int device, IntPtr hardwareIdBuffer, uint bufferSize);

        // interception_is_keyboard and interception_is_mouse are static inline functions
        // in the C header (interception.h), NOT exported from interception.dll.
        // P/Invoke would fail with EntryPointNotFoundException. Implemented in C# instead.
        public static int IsKeyboard(int device)
        {
            return (device >= 1 && device <= MAX_KEYBOARD) ? 1 : 0;
        }

        public static int IsMouse(int device)
        {
            return (device >= MAX_KEYBOARD + 1 && device <= MAX_DEVICE) ? 1 : 0;
        }
    }
}
