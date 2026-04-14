namespace Impostor.Api.Safety.LanguageDetectors;

internal interface IDaterLanguageDetector
{
    bool IsDater(string input, bool useMl, int playerCount);
    DaterCheck.DaterDetectionResult GetDaterDetection(string input, bool useMl, int playerCount);
}
