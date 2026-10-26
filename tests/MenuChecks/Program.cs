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

foreach(var language in QuestionCards.Languages)
{
    var cards=QuestionCards.Get(language.Code);
    if(cards.Count!=8||cards.Select(x=>x.Id).Distinct().Count()!=8||cards.Any(x=>string.IsNullOrWhiteSpace(x.Text)||string.IsNullOrWhiteSpace(x.Turkish)))throw new Exception("Eksik soru kartı: "+language.Code);
    if(language.Code!="tr" && cards.Any(x=>x.Text==x.Turkish))throw new Exception("Yanlış dilde soru kartı: "+language.Code);
}
try { QuestionCards.Get("xx");throw new Exception("Bilinmeyen dil sessizce kabul edildi"); }catch(ArgumentException) { }
if(QuestionCards.Languages.Single(x=>x.Code=="ar").Direction!="rtl")throw new Exception("Arapça yönü");
foreach(var pair in new[]{("en-US","en"),("tr_TR","tr"),("tur","tr"),("jpn","ja"),("ara","ar"),("zho","zh"),("fra","fr"),("deu","de")})
 if(QuestionCards.NormalizeVoiceLanguage(pair.Item1)!=pair.Item2)throw new Exception("Yanlış ses dili: "+pair.Item1);
Console.WriteLine("PASS: 10 dilde sabit kartlar, bilinmeyen dil reddi, RTL ve cihaz ses dili eşleştirme. Çeviri kalitesinin insan incelemesi ayrıca gereklidir.");
