using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using KeyPad2Xbox.Core.Native;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;

namespace KeyPad2Xbox.Core
{
    public class GamepadEngine : IDisposable
    {
        private ViGEmClient? _client;
        private IXbox360Controller? _pad;
        private IntPtr _interceptionContext;
        private readonly KeyMapper _keyMapper;

        // KRİTİK NOT: Delegate'in GC (Garbage Collector) tarafından toplanmasını önlemek için
        // static readonly field olarak tanımlıyoruz. Aksi takdirde AccessViolationException alınır.
        private static readonly InterceptionNative.Predicate _isKeyboard = 
            device => InterceptionNative.interception_is_keyboard(device);

        public GamepadEngine()
        {
            _keyMapper = new KeyMapper();
        }

        public void Initialize()
        {
            // 1. ViGEmClient başlatılıyor
            try
            {
                _client = new ViGEmClient();
                _pad = _client.CreateXbox360Controller();
                _pad.Connect();
            }
            catch (Exception ex)
            {
                throw new Exception("ViGEmBus driver kurulu değil veya başlatılamadı. Hata: " + ex.Message);
            }

            // 2. Interception başlatılıyor
            _interceptionContext = InterceptionNative.interception_create_context();
            if (_interceptionContext == IntPtr.Zero)
            {
                throw new Exception("Interception driver kurulu değil veya sistem reboot edilmedi.");
            }

            // Sadece klavye eventlarını filtrele
            InterceptionNative.interception_set_filter(_interceptionContext, _isKeyboard, InterceptionNative.FILTER_KEY_ALL);
        }

        public List<KeyboardInfo> GetConnectedKeyboards()
        {
            var list = new List<KeyboardInfo>();
            if (_interceptionContext == IntPtr.Zero) return list;

            IntPtr buffer = Marshal.AllocHGlobal(500);
            try
            {
                for (int i = 1; i <= InterceptionNative.MAX_KEYBOARD; i++)
                {
                    int length = InterceptionNative.interception_get_hardware_id(_interceptionContext, i, buffer, 500);
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

            Stroke stroke = new Stroke();

            // 3. Ana Döngü (Main Loop)
            while (true)
            {
                // Blocking wait (tavsiye edilen kullanım)
                int device = InterceptionNative.interception_wait(_interceptionContext);
                
                // Event'i al
                if (InterceptionNative.interception_receive(_interceptionContext, device, ref stroke, 1) > 0)
                {
                    // Eğer hedef klavye değilse ya da bir şekilde mouse eventi ise doğrudan geçir (pass-through)
                    if (InterceptionNative.interception_is_keyboard(device) == 0 || device != targetDeviceId)
                    {
                        InterceptionNative.interception_send(_interceptionContext, device, ref stroke, 1);
                        continue;
                    }

                    // ====== Hedef Klavye İşlemleri ======
                    
                    ushort code = stroke.code;
                    ushort state = stroke.state;

                    // ESC = 0x01 (Uygulamadan temiz çıkış)
                    if (code == 0x01)
                    {
                        Console.WriteLine("[Bilgi] ESC tuşuna basıldı. Uygulamadan çıkılıyor...");
                        break;
                    }

                    // Tuş durumlarını ayrıştırma (State)
                    // KEY_DOWN = 0x00, KEY_UP = 0x01, KEY_E0 = 0x02, KEY_E0_UP = 0x03
                    bool isPressed = (state & 0x01) == 0;
                    bool isExtended = (state & 0x02) != 0;

                    // Mapper üzerinden çeviri yapmayı dene
                    bool mapped = _keyMapper.TryMap(code, isExtended, isPressed, _pad);

                    if (mapped)
                    {
                        // gamepad'e veriyi yolla
                        _pad.SubmitReport();
                        // Tuş mapper tarafından yakalandıysa, interception_send çağırma (tuş yutulur)
                    }
                    else
                    {
                        // Eşleştirilmemiş tuşlar da yutulabilir ama isterseniz Windows'a gönderebilirsiniz.
                        // Talebe göre: "Seçilen klavyenin tuşları Windows'a gitmeyecek (yutulacak)"
                        // Bu yüzden interception_send ÇAĞRILMAYACAK. (Sadece mapping var ise tetiklenir, geri kalan tuşlar void'e düşer)
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_pad != null)
            {
                try { _pad.Disconnect(); } catch { }
                _pad = null;
            }

            if (_client != null)
            {
                try { _client.Dispose(); } catch { }
                _client = null;
            }

            if (_interceptionContext != IntPtr.Zero)
            {
                InterceptionNative.interception_destroy_context(_interceptionContext);
                _interceptionContext = IntPtr.Zero;
            }
        }
    }
}
