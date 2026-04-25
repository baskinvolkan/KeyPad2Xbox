using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using KeyPad2Xbox.Core.Native;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;

namespace KeyPad2Xbox.Core
{
    public class GamepadEngine : IDisposable
    {
        private const int HardwareIdBufferBytes = 512;

        private ViGEmClient? _client;
        private IXbox360Controller? _pad;
        private IntPtr _interceptionContext;
        private volatile bool _stopping;
        private readonly KeyMapper _keyMapper;

        // Delegate must be kept alive — passed to native filter and stored by the DLL.
        // Without a managed reference the GC reclaims it and the next callback hits AVException.
        private static readonly InterceptionNative.Predicate _isKeyboard =
            device => InterceptionNative.IsKeyboard(device);

        public GamepadEngine()
        {
            _keyMapper = new KeyMapper();
        }

        public void Initialize()
        {
            try
            {
                _client = new ViGEmClient();
                _pad = _client.CreateXbox360Controller();
                _pad.Connect();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "ViGEmBus driver kurulu değil veya başlatılamadı.", ex);
            }

            _interceptionContext = InterceptionNative.interception_create_context();
            if (_interceptionContext == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "Interception driver kurulu değil veya sistem reboot edilmedi.");
            }
            // Filter intentionally NOT set here. Setting FILTER_KEY_ALL before the
            // user picks a keyboard would lock both keyboards while we're waiting
            // on Console.ReadLine — the user can't type the device number.
            // The filter is enabled in Start() once the target device is known.
        }

        public List<KeyboardInfo> GetConnectedKeyboards()
        {
            var list = new List<KeyboardInfo>();
            if (_interceptionContext == IntPtr.Zero) return list;

            IntPtr buffer = Marshal.AllocHGlobal(HardwareIdBufferBytes);
            try
            {
                for (int i = 1; i <= InterceptionNative.MAX_KEYBOARD; i++)
                {
                    int length = InterceptionNative.interception_get_hardware_id(
                        _interceptionContext, i, buffer, HardwareIdBufferBytes);
                    if (length > 0)
                    {
                        string hardwareId = Marshal.PtrToStringUni(buffer) ?? "Unknown";
                        list.Add(new KeyboardInfo { DeviceId = i, HardwareId = hardwareId });
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return list;
        }

        public void Start(int targetDeviceId)
        {
            if (_interceptionContext == IntPtr.Zero || _pad == null)
            {
                throw new InvalidOperationException("GamepadEngine is not initialized.");
            }

            Console.WriteLine($"[Bilgi] Hedef klavye cihaz ID'si {targetDeviceId} olarak ayarlandı.");
            Console.WriteLine("[Bilgi] Hedef klavyeden ESC tuşuna basarak çıkabilirsiniz.");
            Console.WriteLine("[Bilgi] Diğer klavyeleriniz normal çalışmaya devam eder.");

            // Activate the filter only now — we want to start swallowing keys
            // exclusively after the user has selected the target device.
            InterceptionNative.interception_set_filter(
                _interceptionContext, _isKeyboard, InterceptionNative.FILTER_KEY_ALL);

            Stroke stroke = new Stroke();

            try
            {
                RunLoop(targetDeviceId, ref stroke);
            }
            finally
            {
                // Clear the filter so non-target keyboards aren't briefly choked
                // during teardown if the context destroy is delayed.
                IntPtr ctx = Volatile.Read(ref _interceptionContext);
                if (ctx != IntPtr.Zero)
                {
                    try
                    {
                        InterceptionNative.interception_set_filter(
                            ctx, _isKeyboard, InterceptionNative.FILTER_KEY_NONE);
                    }
                    catch { }
                }
                ReleaseAllButtons();
            }
        }

        private void RunLoop(int targetDeviceId, ref Stroke stroke)
        {
            while (!_stopping)
            {
                IntPtr ctx = Volatile.Read(ref _interceptionContext);
                if (ctx == IntPtr.Zero) break;

                int device = InterceptionNative.interception_wait(ctx);

                if (_stopping || device == 0) break;

                ctx = Volatile.Read(ref _interceptionContext);
                if (ctx == IntPtr.Zero) break;

                if (InterceptionNative.interception_receive(ctx, device, ref stroke, 1) <= 0)
                {
                    continue;
                }

                bool isTargetKeyboard =
                    InterceptionNative.IsKeyboard(device) != 0 && device == targetDeviceId;

                if (!isTargetKeyboard)
                {
                    InterceptionNative.interception_send(ctx, device, ref stroke, 1);
                    continue;
                }

                ushort code = stroke.code;
                ushort state = stroke.state;

                // ESC = scan code 0x01 → temiz çıkış
                if (code == 0x01)
                {
                    Console.WriteLine("[Bilgi] ESC tuşuna basıldı. Uygulamadan çıkılıyor...");
                    break;
                }

                // KEY_DOWN=0x00, KEY_UP=0x01, KEY_E0=0x02, KEY_E0_UP=0x03
                bool isPressed = (state & 0x01) == 0;
                bool isExtended = (state & 0x02) != 0;

                if (_keyMapper.TryMap(code, isExtended, isPressed, _pad))
                {
                    _pad.SubmitReport();
                }
                // Eşleşmeyen tuşlar bilinçli olarak yutulur — hedef klavye Windows'a yazmamalı.
            }
        }

        private void ReleaseAllButtons()
        {
            try
            {
                _pad?.ResetReport();
                _pad?.SubmitReport();
            }
            catch
            {
                // Pad zaten disconnect olmuş olabilir — yutmak güvenli.
            }
        }

        /// <summary>
        /// Ctrl+C handler'ından çağrılır. interception_wait blocking olduğu için
        /// context'i atomik şekilde devralıp yıkarak ana döngüyü unblock eder.
        /// </summary>
        public void RequestStop()
        {
            _stopping = true;
            IntPtr ctx = Interlocked.Exchange(ref _interceptionContext, IntPtr.Zero);
            if (ctx != IntPtr.Zero)
            {
                InterceptionNative.interception_destroy_context(ctx);
            }
        }

        public void Dispose()
        {
            _stopping = true;

            IntPtr ctx = Interlocked.Exchange(ref _interceptionContext, IntPtr.Zero);
            if (ctx != IntPtr.Zero)
            {
                InterceptionNative.interception_destroy_context(ctx);
            }

            var pad = _pad;
            _pad = null;
            if (pad != null)
            {
                try { pad.Disconnect(); } catch { }
            }

            var client = _client;
            _client = null;
            if (client != null)
            {
                try { client.Dispose(); } catch { }
            }
        }
    }
}
