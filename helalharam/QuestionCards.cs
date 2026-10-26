namespace helalharam;

public sealed record QuestionLanguage(string Code, string Name, string Direction);
public sealed record QuestionCard(string Id, string Turkish, string Text);

// Fixed phrase drafts, independent of menu recognition and machine translation.
public static class QuestionCards
{
    public static IReadOnlyList<QuestionLanguage> Languages { get; } = Array.AsReadOnly(new[] {
        new QuestionLanguage("tr","Türkçe","ltr"), new("en","English","ltr"), new("ar","العربية","rtl"),
        new("fr","Français","ltr"), new("de","Deutsch","ltr"), new("it","Italiano","ltr"),
        new("es","Español","ltr"), new("ja","日本語","ltr"), new("ko","한국어","ltr"), new("zh","中文（简体）","ltr") });
    private static readonly string[] Ids = ["ingredients","pork","meat","alcohol","gelatin","broth","equipment","alternative"];
    private static readonly Dictionary<string,string[]> Texts = new()
    {
        ["tr"] = [
            "Bu yemeğin soslar ve katkı maddeleri dahil tüm içeriklerini söyleyebilir misiniz?",
            "Bu yemekte domuz eti, domuz yağı veya domuzdan elde edilen başka bir bileşen var mı?",
            "Etin tedarikçisi kim? Varsa bu eti kapsayan geçerli helal sertifikasını görebilir miyim?",
            "Yemeğin hazırlanmasında veya soslarında şarap ya da başka bir alkollü içecek kullanılıyor mu?",
            "Jelatin var mı? Varsa bitkisel mi, hayvansal mı? Hayvansalsa hangi hayvandan?",
            "Et suyu veya bulyon kullanılıyor mu? Varsa hangi hayvandan elde ediliyor?",
            "Aynı tava, ızgara, kızartma yağı veya mutfak araçları domuz içeren yiyeceklerde de kullanılıyor mu?",
            "İçeriği belirsiz bileşen çıkarılabilir mi? İçeriklerini açıklayabileceğiniz başka bir yemek önerebilir misiniz?"],
        ["en"] = [
            "Could you list all the ingredients in this dish, including sauces and additives?",
            "Does this dish contain pork, lard, or any other ingredient derived from pigs?",
            "Who supplies the meat? May I see a valid halal certificate that covers this meat, if available?",
            "Is wine or any other alcoholic drink used to prepare this dish or its sauces?",
            "Does it contain gelatin? Is it plant-based or animal-derived? If animal-derived, which animal?",
            "Is meat stock or bouillon used? If so, which animal does it come from?",
            "Are the same pan, grill, frying oil, or utensils also used for foods containing pork?",
            "Can the ingredient with an unknown source be left out? Could you suggest another dish whose ingredients you can explain?"],
        ["ar"] = [
            "هل يمكنكم ذكر جميع مكونات هذا الطبق، بما فيها الصلصات والمواد المضافة؟",
            "هل يحتوي هذا الطبق على لحم الخنزير أو شحمه أو أي مكون آخر مشتق من الخنزير؟",
            "من يورد اللحوم؟ هل يمكنني الاطلاع على شهادة حلال سارية تشمل هذا اللحم، إن وجدت؟",
            "هل يُستخدم النبيذ أو أي مشروب كحولي آخر في تحضير هذا الطبق أو صلصاته؟",
            "هل يحتوي على الجيلاتين؟ هل مصدره نباتي أم حيواني؟ وإذا كان حيوانيًا، فمن أي حيوان؟",
            "هل يُستخدم مرق اللحم أو مكعبات المرق؟ إذا كان كذلك، فمن أي حيوان؟",
            "هل تُستخدم المقلاة أو الشواية أو زيت القلي أو الأدوات نفسها لأطعمة تحتوي على الخنزير؟",
            "هل يمكن حذف المكون مجهول المصدر؟ هل يمكنكم اقتراح طبق آخر تستطيعون توضيح مكوناته؟"],
        ["fr"] = [
            "Pouvez-vous me donner tous les ingrédients de ce plat, y compris les sauces et les additifs ?",
            "Ce plat contient-il du porc, du saindoux ou un autre ingrédient dérivé du porc ?",
            "Qui fournit la viande ? Puis-je voir un certificat halal valide couvrant cette viande, s’il existe ?",
            "Du vin ou une autre boisson alcoolisée est-il utilisé pour préparer ce plat ou ses sauces ?",
            "Ce plat contient-il de la gélatine ? Est-elle d’origine végétale ou animale ? De quel animal provient-elle ?",
            "Utilisez-vous du bouillon de viande ou des cubes de bouillon ? De quel animal proviennent-ils ?",
            "La même poêle, le même gril, la même huile de friture ou les mêmes ustensiles servent-ils aussi aux aliments contenant du porc ?",
            "Peut-on retirer l’ingrédient dont l’origine est inconnue ? Pouvez-vous proposer un autre plat dont vous pouvez expliquer les ingrédients ?"],
        ["de"] = [
            "Können Sie alle Zutaten dieses Gerichts nennen, einschließlich Soßen und Zusatzstoffen?",
            "Enthält dieses Gericht Schweinefleisch, Schweineschmalz oder andere Zutaten vom Schwein?",
            "Wer liefert das Fleisch? Darf ich ein gültiges Halal-Zertifikat sehen, das dieses Fleisch abdeckt, falls vorhanden?",
            "Wird bei der Zubereitung dieses Gerichts oder seiner Soßen Wein oder ein anderes alkoholisches Getränk verwendet?",
            "Enthält es Gelatine? Ist sie pflanzlichen oder tierischen Ursprungs? Falls tierisch: von welchem Tier?",
            "Wird Fleischbrühe oder Brühwürfel verwendet? Von welchem Tier stammt die Brühe?",
            "Werden dieselbe Pfanne, derselbe Grill, dasselbe Frittieröl oder dieselben Utensilien auch für Speisen mit Schweinefleisch verwendet?",
            "Kann die Zutat unbekannter Herkunft weggelassen werden? Können Sie ein anderes Gericht empfehlen, dessen Zutaten Sie erklären können?"],
        ["it"] = [
            "Potete elencare tutti gli ingredienti di questo piatto, comprese salse e additivi?",
            "Questo piatto contiene carne di maiale, strutto o altri ingredienti derivati dal maiale?",
            "Chi fornisce la carne? Posso vedere un certificato halal valido che copra questa carne, se disponibile?",
            "Nella preparazione del piatto o delle salse si usano vino o altre bevande alcoliche?",
            "Contiene gelatina? È di origine vegetale o animale? Se animale, da quale animale proviene?",
            "Si usa brodo di carne o dado? Da quale animale proviene?",
            "La stessa padella, griglia, olio di frittura o gli stessi utensili vengono usati anche per alimenti contenenti maiale?",
            "Si può omettere l’ingrediente di origine sconosciuta? Potete suggerire un altro piatto di cui sapete spiegare gli ingredienti?"],
        ["es"] = [
            "¿Pueden indicar todos los ingredientes de este plato, incluidas las salsas y los aditivos?",
            "¿Este plato contiene cerdo, manteca de cerdo u otro ingrediente derivado del cerdo?",
            "¿Quién suministra la carne? ¿Puedo ver un certificado halal vigente que cubra esta carne, si lo hay?",
            "¿Se utiliza vino u otra bebida alcohólica para preparar este plato o sus salsas?",
            "¿Contiene gelatina? ¿Es de origen vegetal o animal? Si es animal, ¿de qué animal procede?",
            "¿Se utiliza caldo de carne o pastillas de caldo? ¿De qué animal proceden?",
            "¿La misma sartén, parrilla, aceite de fritura o los mismos utensilios se usan también para alimentos que contienen cerdo?",
            "¿Se puede omitir el ingrediente de origen desconocido? ¿Pueden sugerir otro plato cuyos ingredientes puedan explicar?"],
        ["ja"] = [
            "ソースや添加物を含め、この料理のすべての材料を教えていただけますか。",
            "この料理には豚肉、ラード、その他の豚由来の材料が含まれていますか。",
            "肉の仕入れ先はどこですか。この肉を対象とする有効なハラール認証書があれば、見せていただけますか。",
            "この料理やソースの調理に、ワインなどのアルコール飲料を使っていますか。",
            "ゼラチンは含まれていますか。植物由来ですか、動物由来ですか。動物由来の場合、何の動物ですか。",
            "肉のだしや固形ブイヨンを使っていますか。何の動物から作られていますか。",
            "同じフライパン、グリル、揚げ油、調理器具を、豚由来の材料を含む食品にも使っていますか。",
            "由来が不明な材料を除くことはできますか。材料を説明できる別の料理を勧めていただけますか。"],
        ["ko"] = [
            "소스와 첨가물을 포함하여 이 요리의 모든 재료를 알려 주실 수 있나요?",
            "이 요리에 돼지고기, 돼지기름 또는 다른 돼지 유래 성분이 들어 있나요?",
            "고기 공급업체는 어디인가요? 이 고기에 적용되는 유효한 할랄 인증서가 있다면 볼 수 있나요?",
            "이 요리나 소스를 만들 때 와인이나 다른 알코올 음료를 사용하나요?",
            "젤라틴이 들어 있나요? 식물성인가요, 동물성인가요? 동물성이라면 어떤 동물에서 얻었나요?",
            "고기 육수나 고형 육수를 사용하나요? 어떤 동물에서 얻은 것인가요?",
            "같은 팬, 그릴, 튀김 기름이나 조리 도구를 돼지 유래 재료가 들어간 음식에도 사용하나요?",
            "출처가 불분명한 재료를 빼 주실 수 있나요? 재료를 설명할 수 있는 다른 요리를 추천해 주실 수 있나요?"],
        ["zh"] = [
            "可以告诉我这道菜的所有配料，包括酱汁和添加剂吗？",
            "这道菜含有猪肉、猪油或其他猪源成分吗？",
            "肉类供应商是谁？如果有涵盖这种肉的有效清真认证，可以给我看一下吗？",
            "制作这道菜或酱汁时，是否使用葡萄酒或其他酒精饮料？",
            "里面有明胶吗？是植物来源还是动物来源？如果是动物来源，来自什么动物？",
            "是否使用肉汤或浓汤块？来自什么动物？",
            "同一个锅、烤架、炸油或厨具，是否也用于含有猪源成分的食物？",
            "可以去掉来源不明的配料吗？可以推荐另一道能说明配料的菜吗？"]
    };
    public static IReadOnlyList<QuestionCard> Get(string language)
    {
        if (!Texts.TryGetValue(language, out var values)) throw new ArgumentException("Desteklenmeyen soru kartı dili.", nameof(language));
        return Ids.Select((id,i)=>new QuestionCard(id,Texts["tr"][i],values[i])).ToArray();
    }
    public static string NormalizeVoiceLanguage(string? language)
    {
        var code=(language??"").Replace('_','-').Split('-')[0].ToLowerInvariant();
        return code switch { "tur"=>"tr", "eng"=>"en", "ara"=>"ar", "fra" or "fre"=>"fr", "deu" or "ger"=>"de", "ita"=>"it", "spa"=>"es", "jpn"=>"ja", "kor"=>"ko", "zho" or "chi"=>"zh", _=>code };
    }
}
