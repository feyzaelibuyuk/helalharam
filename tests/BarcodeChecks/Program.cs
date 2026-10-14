using helalharam;
foreach(var valid in new[]{"3017620422003","034000470693","12345678","03017620422003"," 3017620422003 "})
    if(!ProductBarcode.IsValid(valid)) throw new Exception("Geçerli barkod reddedildi");
foreach(var invalid in new string?[]{null,"","https://example.com","1234567","123456789012345","１２３４５６７８","12345678\nextra"})
    if(ProductBarcode.IsValid(invalid)) throw new Exception("Ürün olmayan kod kabul edildi");
Console.WriteLine("PASS: ürün barkod uzunlukları, baştaki sıfırlar, ASCII rakamlar ve QR/metin reddi.");
