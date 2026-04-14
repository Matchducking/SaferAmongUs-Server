namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterItalianLanguageDetector : DaterLanguageDetectorBase
{
    public DaterItalianLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "amore", "coppia", "ragazzo", "ragazza", "eta", "cerco", "single", "incontro", "incontri", "amicodiletto", "amicadiletto", "letto", "scopamico", "scopamica", "trombamico", "trombamica", "sesso", "sext", "caldo", "calda", "privato", "scrivimi", "snap", "telegram", "whatsapp", "dm", "insta", "voglios", "dotat" }, useCore: true, mlModel: mlModel)
    {
    }
}
