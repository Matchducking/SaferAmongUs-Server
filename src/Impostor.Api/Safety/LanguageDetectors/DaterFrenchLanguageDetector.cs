namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterFrenchLanguageDetector : DaterLanguageDetectorBase
{
    public DaterFrenchLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "amour", "couple", "copain", "copine", "age", "rencontre", "rencontres", "celib", "flirt", "coquin", "chaud", "sexe", "sexto", "plancul", "cul", "baise", "sexfriend", "snap", "telegram", "whatsapp", "mp", "dm", "insta" }, useCore: true, mlModel: mlModel)
    {
    }
}
