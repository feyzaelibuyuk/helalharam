using helalharam;
foreach (var (text, language) in new[] { ("Tavuk çorba", "tr"), ("Chicken soup", "en"), ("Poulet sauce", "fr"), ("Hähnchen Suppe", "de") })
{
    var output = MenuQuestions.Prepare(text, language);
    if (!output.Contains("Etin tedarikçisi") || !output.Contains("Et suyu")) throw new Exception("Et/bulyon sorusu eksik: " + language);
}
if (MenuQuestions.Prepare("Etiket sebze", "tr").Contains("Etin tedarikçisi")) throw new Exception("Etiket sözcüğü et sayıldı");
if (!MenuQuestions.Prepare("Cake wine", "en").Contains("alkol kullanılıyor")) throw new Exception("Alkol sorusu eksik");
if (!MenuQuestions.Prepare("外国菜", "other").Contains("genel sorular")) throw new Exception("Desteklenmeyen dil yanlış değerlendirildi");
if (!MenuQuestions.Prepare("", "tr").Contains("Önce")) throw new Exception("Boş metin kontrolü eksik");
if (!MenuQuestions.Prepare("JELATİN", "tr").Contains("Jelatin veya")) throw new Exception("Türkçe büyük İ eşleşmedi");
if (!MenuQuestions.Prepare("PİLAV", "tr").Contains("Et suyu")) throw new Exception("Türkçe büyük İ bulyon eşleşmedi");
if (!MenuQuestions.Prepare("TATLI", "tr").Contains("Jelatin veya")) throw new Exception("Türkçe büyük I eşleşmedi");
Console.WriteLine("PASS: dört dil, sözcük sınırları, alkol, boş metin ve bilinmeyen dil.");
