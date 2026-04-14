namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterFilipinoLanguageDetector : DaterLanguageDetectorBase
{
    public DaterFilipinoLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "jowa", "hanap", "hanapjowa", "edad", "lalaki", "babae", "landi", "fubu", "fwb", "pm", "pmmo", "chat", "dm", "insta", "telegram", "whatsapp", "sexy", "nudes", "kantot", "kantutan" }, useCore: true, mlModel: mlModel)
    {
    }
}
