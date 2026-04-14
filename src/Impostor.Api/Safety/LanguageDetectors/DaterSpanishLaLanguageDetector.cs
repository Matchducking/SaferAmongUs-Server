namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterSpanishLaLanguageDetector : DaterLanguageDetectorBase
{
    public DaterSpanishLaLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(
            new[] {
                "novia", "novio", "pareja", "busco", "buscando", "edad", "anos", "chico", "chica", "solter", "amor", "ligue", "ligar", "cita", "citas",
                "follamigo", "follamiga", "amigovio", "amigovia", "chongo", "vrga", "verga", "papi", "sexo", "sext", "caliente", "morb", "pack", "nudes",
                "priv", "pasainsta", "agregame", "dm", "insta", "telegram", "whats", "wasap", "sub", "chupo", "bucet", "vagin", "bucetuda", "bucetudo",
                "xot", "xota", "pic", "pica", "piroc", "pta", "puta", "put", "cavalo", "cvlo", "rld", "hijo", "pad", "moren", "loir", "amg", "gtso", "amig",
                "culon", "culona", "cvlon", "cvlona", "comeburra", "burra", "papaya", "melon", "melones", "meloneshot", "bsc", "vaj", "vajina",
                "pene", "penegr", "pn3gr4nd", "pnegran", "pnexzan", "tumoren", "morenoo",
                "coje", "coj", "qlon", "qlo", "facux", "f4cux",
            },
            useCore: true,
            mlModel: mlModel)
    {
    }
}
