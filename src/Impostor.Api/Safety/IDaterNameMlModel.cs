using System.Reflection;

namespace Impostor.Api.Safety;


internal interface IDaterNameMlModel
{
    bool IsLikelyDater(string input, int playerCount = 0);
}
