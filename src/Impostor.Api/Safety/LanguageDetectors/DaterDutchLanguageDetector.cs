namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterDutchLanguageDetector : DaterLanguageDetectorBase
{
    public DaterDutchLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "liefde", "koppel", "vriendin", "vriend", "leeftijd", "zoek", "single", "daten", "seks", "sekschat", "seksdate", "seksvriend", "seksmaatje", "fuckbuddy", "fwb", "neuk", "neuken", "heet", "priv", "prive", "snap", "telegram", "whatsapp", "dm", "insta" }, useCore: true, mlModel: mlModel)
    {
    }
}
