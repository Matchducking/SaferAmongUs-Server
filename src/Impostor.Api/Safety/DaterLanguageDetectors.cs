using Impostor.Api.Innersloth;
using Impostor.Api.Safety.LanguageDetectors;

namespace Impostor.Api.Safety;

internal static class DaterLanguageDetectors
{
    private static readonly IDaterLanguageDetector English = new DaterEnglishLanguageDetector(DaterCheck.DefaultMlModel);
    private static readonly IDaterLanguageDetector Spanish = new DaterSpanishLaLanguageDetector();
    private static readonly IDaterLanguageDetector Korean = new DaterKoreanLanguageDetector();
    private static readonly IDaterLanguageDetector Russian = new DaterRussianLanguageDetector();
    private static readonly IDaterLanguageDetector Portuguese = new DaterPortugueseLanguageDetector();
    private static readonly IDaterLanguageDetector Arabic = new DaterArabicLanguageDetector();
    private static readonly IDaterLanguageDetector Filipino = new DaterFilipinoLanguageDetector();
    private static readonly IDaterLanguageDetector Polish = new DaterPolishLanguageDetector();
    private static readonly IDaterLanguageDetector Japanese = new DaterJapaneseLanguageDetector();
    private static readonly IDaterLanguageDetector Dutch = new DaterDutchLanguageDetector();
    private static readonly IDaterLanguageDetector French = new DaterFrenchLanguageDetector();
    private static readonly IDaterLanguageDetector German = new DaterGermanLanguageDetector();
    private static readonly IDaterLanguageDetector Italian = new DaterItalianLanguageDetector();
    private static readonly IDaterLanguageDetector Chinese = new DaterSimplifiedChineseLanguageDetector();
    private static readonly IDaterLanguageDetector Irish = new DaterIrishLanguageDetector();

    public static IDaterLanguageDetector Resolve(GameKeywords keywords)
    {
        if (keywords == GameKeywords.All || keywords == GameKeywords.Other)
        {
            return English;
        }

        if (Has(keywords, GameKeywords.SpanishLA)) return Spanish;
        if (Has(keywords, GameKeywords.SpanishEU)) return Spanish;
        if (Has(keywords, GameKeywords.Korean)) return Korean;
        if (Has(keywords, GameKeywords.Russian)) return Russian;
        if (Has(keywords, GameKeywords.Portuguese)) return Portuguese;
        if (Has(keywords, GameKeywords.Arabic)) return Arabic;
        if (Has(keywords, GameKeywords.Filipino)) return Filipino;
        if (Has(keywords, GameKeywords.Polish)) return Polish;
        if (Has(keywords, GameKeywords.Japanese)) return Japanese;
        if (Has(keywords, GameKeywords.Brazilian)) return Portuguese;
        if (Has(keywords, GameKeywords.Dutch)) return Dutch;
        if (Has(keywords, GameKeywords.French)) return French;
        if (Has(keywords, GameKeywords.German)) return German;
        if (Has(keywords, GameKeywords.Italian)) return Italian;
        if (Has(keywords, GameKeywords.SChinese)) return Chinese;
        if (Has(keywords, GameKeywords.TChinese)) return Chinese;
        if (Has(keywords, GameKeywords.Irish)) return Irish;
        if (Has(keywords, GameKeywords.English)) return English;

        return English;
    }

    private static bool Has(GameKeywords value, GameKeywords flag)
    {
        return (value & flag) == flag;
    }
}
