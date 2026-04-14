namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterRussianLanguageDetector : DaterLanguageDetectorBase
{
    public DaterRussianLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "девуш", "девушк", "парен", "парн", "ищу", "знаком", "знакомства", "лет", "возраст", "любов", "пиши", "лс", "телег", "телега", "секс", "сексчат", "нюд", "нюдс", "вирт", "интим", "тг", "пошл", "пошлу", "ищупошл" }, useCore: true, mlModel: mlModel)
    {
    }
}
