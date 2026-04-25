Bu klasör Interception API'sinin çalışması için gereken C kütüphanesi olan `interception.dll` dosyasını tutmalıdır. Lisans ve mimari sebeplerinden dolayı bu dosyayı manuel olarak indirmeniz gerekmektedir.

## Nasıl Eklenir?
1. [Interception Releases](https://github.com/oblitum/Interception/releases) sayfasından en güncel sürümü (genellikle 1.0.1) indirin.
2. `.zip` dosyasını bir klasöre çıkartın.
3. `Interception/library/x64/interception.dll` dizinindeki 64-bit DLL dosyasını kopyalayın.
4. Bu klasöre (`lib/`) yapıştırın.

> **Uyarı:** `interception.dll` eklendiğinde git tarafından takip edilecek şekilde `.gitignore` içerisinde kısıtlanmamıştır. Projeyi build/publish ettiğinizde program dosyalarının olduğu yere otomatik kopyalanacaktır.
