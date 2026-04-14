namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterPolishLanguageDetector : DaterLanguageDetectorBase
{
    public DaterPolishLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "milosc", "para", "chlopak", "chlopaka", "dziewczyna", "dziewczyn", "wiek", "szukam", "randka", "randki", "seks", "sext", "fuckfriend", "fwb", "pisz", "priv", "snap", "telegram", "whatsapp", "dm", "insta" }, useCore: true, mlModel: mlModel)
    {
    }
}
