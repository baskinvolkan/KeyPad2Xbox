using System;
using KeyPad2Xbox.Core;

namespace KeyPad2Xbox.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== KeyPad2Xbox Başlatılıyor ===");

            using var engine = new GamepadEngine();
            try
            {
                engine.Initialize();
                Console.WriteLine("[Bilgi] ViGEm ve Interception başarıyla başlatıldı.");

                var keyboards = engine.GetConnectedKeyboards();
                if (keyboards.Count == 0)
                {
                    Console.WriteLine("[Hata] Hiç klavye bulunamadı veya Interception driver tetiklenmiyor.");
                    return;
                }

                Console.WriteLine("\n[Klavyeler]");
                for (int i = 0; i < keyboards.Count; i++)
                {
                    Console.WriteLine($"[{i + 1}] {keyboards[i]}");
                }

                int selectedDeviceId = -1;
                while (selectedDeviceId == -1)
                {
                    Console.Write("\nLütfen Xbox Controller'a çevrilecek klavyenin numarasını girin: ");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out int choice) && choice >= 1 && choice <= keyboards.Count)
                    {
                        selectedDeviceId = keyboards[choice - 1].DeviceId;
                    }
                    else
                    {
                        Console.WriteLine("Geçersiz giriş, tekrar deneyin.");
                    }
                }

                // Ctrl+C ile temiz çıkış: interception_wait blocking olduğu için
                // context'i yıkarak unblock etmemiz gerekiyor, aksi halde Dispose çalışmaz.
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true; // Hemen kapanmayı engelle, cleanup'ın çalışmasını bekle
                    Console.WriteLine("\n[Bilgi] Ctrl+C algılandı, çıkılıyor...");
                    engine.RequestStop();
                };

                engine.Start(selectedDeviceId);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Kritik Hata] {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nNot: Uygulamayı Administrator (Yönetici) olarak çalıştırmayı unutmayın.");
            }
            finally
            {
                Console.WriteLine("Temizlik işlemi yapılıyor...");
                // using bloğu GamepadEngine'in Dispose metodunu otomatik çağırıp cleanup yapacaktır.
            }
        }
    }
}
