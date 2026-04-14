using System;
using System.Collections.Generic;

namespace Impostor.Api.Safety.LanguageDetectors;

internal abstract class DaterLanguageDetectorBase : IDaterLanguageDetector
{
    protected DaterLanguageDetectorBase(string[] tokens, bool useCore, IDaterNameMlModel? mlModel)
    {
        _tokens = tokens;
        _useCore = useCore;
        _mlModel = mlModel ?? DaterCheck.DefaultMlModel;
    }

    private readonly string[] _tokens;
    private readonly bool _useCore;
    public IDaterNameMlModel? NameModel => _mlModel;
    private readonly IDaterNameMlModel? _mlModel;

    public virtual bool IsDater(string input, bool useMl, int playerCount)
    {
        return GetDaterDetection(input, useMl, playerCount).Detected;
    }

    public virtual DaterCheck.DaterDetectionResult GetDaterDetection(string input, bool useMl, int playerCount)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return DaterCheck.DaterDetectionResult.NotDetected;
        }

        string rawLower = input.ToLowerInvariant();
        string rawLeet = DaterCheck.NormalizeLeetAscii(rawLower);
        string normalized = DaterCheck.RemoveDiacritics(input).ToLowerInvariant();
        string normalizedLeet = DaterCheck.NormalizeLeetAscii(normalized);

        string tokenEvidence = GetMatchingTokenEvidence(rawLower, rawLeet, normalized, normalizedLeet, _tokens);
        if (!string.IsNullOrEmpty(tokenEvidence))
        {
            return DaterCheck.DaterDetectionResult.Algorithm("language-token", tokenEvidence);
        }

        if (!_useCore)
        {
            if (useMl && _mlModel is not null && _mlModel.IsLikelyDater(input, playerCount))
            {
                return DaterCheck.DaterDetectionResult.MachineLearning("detector-ml");
            }

            return DaterCheck.DaterDetectionResult.NotDetected;
        }

        bool coreDetectedWithoutMl = DaterCheck.IsDaterCore(input, useML: false, playerCount, _mlModel, out string coreEvidence);
        if (coreDetectedWithoutMl)
        {
            return DaterCheck.DaterDetectionResult.Algorithm("core-rules", string.IsNullOrWhiteSpace(coreEvidence) ? "core-rule" : coreEvidence);
        }

        if (useMl && _mlModel is not null && DaterCheck.IsDaterCore(input, useML: true, playerCount, _mlModel, out string mlOrEvidence))
        {
            if (!string.Equals(mlOrEvidence, "ml", StringComparison.Ordinal))
            {
                return DaterCheck.DaterDetectionResult.Algorithm("core-rules", string.IsNullOrWhiteSpace(mlOrEvidence) ? "core-rule" : mlOrEvidence);
            }

            return DaterCheck.DaterDetectionResult.MachineLearning("core-ml-fallback");
        }

        return DaterCheck.DaterDetectionResult.NotDetected;
    }

    protected static string GetMatchingTokenEvidence(string rawLower, string rawLeet, string normalized, string normalizedLeet, string[] tokens)
    {
        if (tokens.Length == 0)
        {
            return string.Empty;
        }

        var matches = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < tokens.Length; i++)
        {
            if (rawLower.Contains(tokens[i], StringComparison.Ordinal) ||
                rawLeet.Contains(tokens[i], StringComparison.Ordinal) ||
                normalized.Contains(tokens[i], StringComparison.Ordinal) ||
                normalizedLeet.Contains(tokens[i], StringComparison.Ordinal))
            {
                matches.Add(tokens[i]);
                if (matches.Count >= 5)
                {
                    break;
                }
            }
        }

        if (matches.Count == 0)
        {
            return string.Empty;
        }

        return string.Join("|", matches);
    }
}
