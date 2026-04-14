namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterIrishLanguageDetector : DaterLanguageDetectorBase
{
    public DaterIrishLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "gra", "grá", "aois", "caidreamh", "leannan", "leannán", "buachaill", "cailin", "dm", "insta", "telegram", "snap" }, useCore: true, mlModel: mlModel)
    {
    }
}
