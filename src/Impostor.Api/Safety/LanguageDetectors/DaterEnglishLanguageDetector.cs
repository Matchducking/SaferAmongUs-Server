using System;

namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterEnglishLanguageDetector : DaterLanguageDetectorBase
{
    public DaterEnglishLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(Array.Empty<string>(), useCore: true, mlModel: mlModel)
    {
    }
}
