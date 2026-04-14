namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterGermanLanguageDetector : DaterLanguageDetectorBase
{
    public DaterGermanLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "liebe", "paar", "freundin", "freund", "alter", "suche", "single", "dating", "date", "fickfreund", "fickfreundin", "sexfreund", "sexfreundin", "sext", "sexchat", "heiss", "pn", "snap", "telegram", "whatsapp", "dm", "insta" }, useCore: true, mlModel: mlModel)
    {
    }
}
