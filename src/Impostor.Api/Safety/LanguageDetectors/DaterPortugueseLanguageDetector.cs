namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterPortugueseLanguageDetector : DaterLanguageDetectorBase
{
    public DaterPortugueseLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(
            new[] {
                "namoro", "namorar", "nm", "nmrd", "namorad", "procuro", "procurando", "idade", "anos", "amizade", "amizades", "solteir", "casal", "amor",
                "garoto", "garota", "menino", "menina", "novinho", "novinha", "ficante", "ficantes", "ficar", "ficando", "peguete", "contatinho", "foda",
                "fixa", "zap", "zapzap", "wpp", "whats", "chama", "chamapv", "chamazap", "mechamapv", "pv", "dm", "direct", "insta", "telegram", "nude",
                "nudes", "nud", "nudz", "sexo", "sext", "safad", "safadinho", "safadinha", "gostoso", "gostosa", "ghosto", "gost", "quero", "bct", "filh", "ROLUD",
                "pau", "pauzudo", "bi", "sub", "irm", "sfd", "priv", "pvt", "papi", "prim", "prima", "primo", "passiv", "passivo", "passiva", "nerd",
                "chup", "chupo", "bucet", "vagin", "bucetuda", "bucetudo", "xot", "xota", "pic", "pica", "piroc", "pta", "puta", "put", "cavalo", "cvlo",
                "rld", "pad", "moren", "loir", "busco", "bsc", "rbd", "rabud", "bund", "nde", "pne",
                "troco", "troc", "ndes"
            },
            useCore: true,
            mlModel: mlModel)
    {
    }
}
