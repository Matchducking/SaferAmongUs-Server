namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterJapaneseLanguageDetector : DaterLanguageDetectorBase
{
    public DaterJapaneseLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "恋愛", "彼氏", "彼女", "年齢", "出会", "出会い", "フレ", "セフレ", "セフ募", "えろ", "エロ", "オフパコ", "裏垢", "うらあか", "会いたい", "通話", "通話相手", "ライン交換", "ディーエム", "ライン", "インスタ", "テレグラム", "カカオ" }, useCore: true, mlModel: mlModel)
    {
    }
}
