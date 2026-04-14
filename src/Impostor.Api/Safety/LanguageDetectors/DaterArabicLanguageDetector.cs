namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterArabicLanguageDetector : DaterLanguageDetectorBase
{
    public DaterArabicLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "حب", "تعارف", "زواج", "عمر", "سن", "ولد", "بنت", "ابحث", "اريد", "خاص", "تعال", "تعالي", "انستا", "واتس", "سناب", "تلجرام", "تيلي", "جنس", "سكس" }, useCore: true, mlModel: mlModel)
    {
    }
}
