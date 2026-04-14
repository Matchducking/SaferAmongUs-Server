using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace Impostor.Api.Safety;


public class DaterNameMLCheck : IDaterNameMlModel, IDisposable
{
    private readonly InferenceSession _session;
    private readonly string _vocabPath;
    private readonly string _mergesPath;
    private readonly ThreadLocal<BpeTokenizer> _tokenizerPerThread;
    private readonly SemaphoreSlim _inferenceGate;

    private const float UnsafeThresholdDefault = 0.92f;
    private const float QuestionableThresholdDefault = 0.97f;
    private const float UnsafeThresholdSixPlayers = 0.86f;
    private const float QuestionableThresholdSixPlayers = 0.93f;
    private const float UnsafeThresholdSmallLobby = 0.80f;
    private const float QuestionableThresholdSmallLobby = 0.88f;

    public DaterNameMLCheck(string modelDir)
    {
        _session = new InferenceSession(Path.Combine(modelDir, "model.onnx"));
        _vocabPath = Path.Combine(modelDir, "vocab.json");
        _mergesPath = Path.Combine(modelDir, "merges.txt");

        _tokenizerPerThread = new ThreadLocal<BpeTokenizer>(CreateTokenizer);

        // Prevent runaway parallel ONNX runs from stalling the process under load.
        int maxParallelInference = Math.Clamp(Environment.ProcessorCount - 1, 2, 12);
        _inferenceGate = new SemaphoreSlim(maxParallelInference, maxParallelInference);
    }

    public bool IsLikelyDater(string input, int playerCount = 0)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        _inferenceGate.Wait();
        try
        {
            var tokenizer = _tokenizerPerThread.Value ?? CreateTokenizer();
            var encoded = tokenizer.EncodeToIds(input);

            // RoBERTa expects: <s> tokens </s>
            // <s> = 0, </s> = 2
            var inputIds = new long[encoded.Count + 2];
            inputIds[0] = 0; // <s>
            for (int i = 0; i < encoded.Count; i++)
                inputIds[i + 1] = (long)encoded[i];
            inputIds[^1] = 2; // </s>

            var seqLen = inputIds.Length;
            var attentionMask = Enumerable.Repeat(1L, seqLen).ToArray();

            var inputIdsTensor = new DenseTensor<long>(inputIds, new[] { 1, seqLen });
            var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, seqLen });

            var inputs = new[]
            {
                NamedOnnxValue.CreateFromTensor("input_ids",      inputIdsTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
            };

            using var results = _session.Run(inputs);
            var logits = results[0].AsEnumerable<float>().ToArray();
            var probs = Softmax(logits);

            (float unsafeThreshold, float questionableThreshold) = GetThresholdsForPlayerCount(playerCount);
            return probs[2] >= unsafeThreshold || probs[1] >= questionableThreshold;
        }
        finally
        {
            _inferenceGate.Release();
        }
    }

    private static (float unsafeThreshold, float questionableThreshold) GetThresholdsForPlayerCount(int playerCount)
    {
        if (playerCount > 0 && playerCount <= 5)
        {
            return (UnsafeThresholdSmallLobby, QuestionableThresholdSmallLobby);
        }

        if (playerCount == 6)
        {
            return (UnsafeThresholdSixPlayers, QuestionableThresholdSixPlayers);
        }

        return (UnsafeThresholdDefault, QuestionableThresholdDefault);
    }

    private BpeTokenizer CreateTokenizer()
    {
        return BpeTokenizer.Create(_vocabPath, _mergesPath);
    }

    private static float[] Softmax(float[] logits)
    {
        float max = logits[0];
        foreach (var l in logits)
            if (l > max) max = l;

        float sum = 0f;
        var exps = new float[logits.Length];
        for (int i = 0; i < logits.Length; i++)
        {
            exps[i] = MathF.Exp(logits[i] - max);
            sum += exps[i];
        }
        for (int i = 0; i < exps.Length; i++)
            exps[i] /= sum;

        return exps;
    }

    public void Dispose()
    {
        _inferenceGate?.Dispose();
        _tokenizerPerThread?.Dispose();
        _session?.Dispose();
        GC.SuppressFinalize(this);
    }
}
