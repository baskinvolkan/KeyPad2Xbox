# KeyPad2Xbox

Windows 11 üzerinde çalışan, iki klavyeden birini sanal bir Xbox 360 controller'a çeviren C# .NET 8 konsol aracı.
Seçilen klavyenin tuşları "yutulur" ve harfler Windows'a ulaşmaz (sadece gamepad komutlarına dönüşür). Diğer klavye normal yazım için çalışmaya devam eder.

## Ön Kurulum Gereksinimleri

Bu projenin Windows makinede çalışabilmesi için iki temel driver kurulumu ZORUNLUDUR:

### Adım 1: ViGEmBus Driver Kurulumu
1. [ViGEmBus Releases](https://github.com/nefarius/ViGEmBus/releases) sayfasına gidin (Tercihen v1.22.0).
2. Yükleyici dosyasını indirin ve kurun.
3. Bu driver bilgisayarınızda sanal Xbox 360 kontrolcüleri oluşturmamıza izin verir.

### Adım 2: Interception Driver Kurulumu
1. [Interception Releases](https://github.com/oblitum/Interception/releases) sayfasına gidin ve son sürümü indirin.
2. Zipten çıkardıktan sonra Administrator (Yönetici) olarak komut istemi (cmd) açın.
3. Çıkarttığınız dizinde `command line installer` klasörüne girip şu komutu çalıştırın:
   `install-interception.exe /install`
4. **BİLGİSAYARI YENİDEN BAŞLATIN!** (Reboot yapmadığınız takdirde `interception_create_context()` null/sıfır dönecektir.)

### Adım 3: interception.dll Dosyasının Yerleştirilmesi
`lib/README.md` dosyasında açıklandığı gibi; indirdiğiniz zip içerisindeki `library/x64/interception.dll` dosyasını bu projedeki `lib/` klasörüne kopyalayın.

---

## Build Talimatları (Windows'ta)

Projeyi derlemek için Windows 11 makinenizde **.NET 8.0 SDK** kurulu olmalıdır. Eğer kurulu değilse, herhangi bir indirme işlemini beklemeden PowerShell üzerinden tek satırla kurabilirsiniz:
```powershell
winget install Microsoft.DotNet.SDK.8
```
*(Kurulum tamamlandıktan sonra açık olan komut istemcisini kapatıp yeniden açmayı unutmayın.)*

Ardından yeni açtığınız komut satırı penceresinde proje dizinine giderek projeyi derleyebilirsiniz:

```cmd
dotnet publish src/KeyPad2Xbox.Console -r win-x64 -c Release --self-contained
```

---

## Çalıştırma
1. Yayınlanmış `KeyPad2Xbox.Console.exe` uygulamasını **SAĞ TIK > YÖNETİCİ OLARAK ÇALIŞTIR (Run as Administrator)** seçeneğiyle açın (Interception yetki gerektirir!).
2. Ekranda algılanan klavye cihazları listelenecektir. `(Örn: [1] device #2 - HID\VID_046D... )`
3. Xbox Controller ile eşleşmesini istediğiniz klavyenin sıra numarasını girin.
4. O klavye artık bir controller oldu! Tuş eşleşmelerini kullanarak oyunlara girebilirsiniz. Çıkmak için o klavyeden **ESC** tuşuna basmanız yeterlidir.

---

## Tuş Haritası (Sabitlenmiş)

| Tuş | Xbox Buton |
|-----|------------|
| ↑ (Yukarı) | D-Pad Up |
| ↓ (Aşağı) | D-Pad Down |
| ← (Sola) | D-Pad Left |
| → (Sağa) | D-Pad Right |
| Q | Left Shoulder (LB) |
| W | Y |
| E | Right Shoulder (RB) |
| R | Right Thumb (R3) |
| A | X |
| S | A |
| D | B |
| F | Left Thumb (L3) |
| ESC | (Uygulamadan Çıkış) |

---

## Sorun Giderme

- **"Interception bulunamadı" veya "Driver kurulu değil" hatası:** Interception kurulumundan sonra "Yeniden Başlat" adımını atlamış olabilirsiniz. Makineyi yeniden başlatın.
- **"ViGEmBus driver kurulu değil" hatası:** ViGEmBus yüklenmemiş veya güncel olmayabilir.
- **Tuşlar hâlâ ekrana yazılıyor / yutulmuyor:** Uygulamayı "Yönetici (Administrator)" olarak çalıştırmadınız demektir. Interception driver'ının engelleyici modülüne müdahale edemiyordur.
- **Uygulama çalışırken aniden çöktü (AccessViolationException):** Bu bir pointer hatasıdır (Genelde Delegate'in GC'ye toplanmasından veya yanlış struct boyutlarından kaynaklanır). Eğer tekrar ederse Bug bildirmekte özgürsünüz ancak kaynak koddaki static referansla bu durum Fixlenmiştir.
- **Ultra Street Fighter IV Uyumluluğu:** Uygulama özellikle USF4 oynamak için tasarlanmıştır. Bu oyunda Interception veya donanım simülasyonunu engelleyen sıkı bir Anti-Cheat mekanizması (Vanguard vb.) bulunmadığından herhangi bir kısıtlama veya engelleme yaşamazsınız.
