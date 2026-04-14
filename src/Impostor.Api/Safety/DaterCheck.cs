using Impostor.Api.Innersloth;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Impostor.Api.Safety.LanguageDetectors;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Impostor.Api.Safety;

public static partial class DaterCheck
{
    public enum DaterDetectionSource
    {
        None,
        Algorithm,
        MachineLearning
    }

    public readonly record struct DaterDetectionResult(bool Detected, DaterDetectionSource Source, string Path, string Evidence)
    {
        public static readonly DaterDetectionResult NotDetected = new(false, DaterDetectionSource.None, "none", string.Empty);

        public static DaterDetectionResult Algorithm(string path, string evidence)
        {
            return new DaterDetectionResult(true, DaterDetectionSource.Algorithm, path, evidence);
        }

        public static DaterDetectionResult MachineLearning(string path)
        {
            return new DaterDetectionResult(true, DaterDetectionSource.MachineLearning, path, "ml");
        }
    }

    // Manual list of known dater names observed through chat.
    public static string[] chatDaterNames = { "faer3635" };
    private static readonly HashSet<string> HornyNames2 = new HashSet<string> { "0cm", "0in", "0lim", "0ln", "0plus", "1cm", "1in", "1ln", "1plus", "2cm", "2in", "2ln", "2plus", "3cm", "3in", "3ln", "3plus", "3som", "3sum", "40rn", "4cm", "4in", "4ln", "4orn", "4plus", "4rni", "4rny", "4som", "4sum", "4xrn", "53x", "5cm", "5ex", "5in", "5ln", "5plus", "5som", "5sum", "6cm", "6in", "6ln", "6plus", "6som", "6sum", "7cm", "7in", "7ln", "7plus", "7som", "7sum", "8cm", "8in", "8ln", "8plus", "8som", "8sum", "9cm", "9in", "9ln", "9plus", "9som", "9sum", "aizawa", "adris", "asx", "aunt", "b0bs", "b0i", "b0ob", "b0xb", "b0y", "babe", "babhi", "badie", "bady", "baku", "banda", "bangali", "bdsm", "bdzm", "beautiful", "bemine", "bengali", "beur", "bewb", "beyour", "bhabhi", "bhabi", "bich", "bick", "bicx", "bigblack", "bigd", "bigdk", "bigwhite", "bitch", "bitcxh", "bitxch", "bitxh", "bixch", "bixk", "bixtch", "blackd", "bner", "bnft", "bnxer", "bo0b", "bobs", "boner", "bonr", "bonxr", "boxbs", "boxy", "bproy", "brcked", "bricked", "brxcked", "brxe", "bsdm", "btch", "bubs", "busx", "buxs", "bwc", "bx0b", "bxbs", "bxby", "bxck", "bxi", "bxitch", "bxner", "bxob", "bxoxy", "bxoy", "bxre", "bxrx", "bxtch", "bxy", "chat", "chdu", "chica", "chico", "chod", "chuda", "chura", "chxdu", "chxt", "cndm", "cndom", "cndxm", "cnm", "condm", "condom", "condxm", "corx", "coxk", "coxn", "coxrn", "coxrx", "creampi", "creamy", "crexm", "crxam", "crxm", "cuck", "cum", "cunt", "cunxt", "cutie", "cuxnt", "cuxt", "cxck", "cxm", "cxndm", "cxndom", "cxnt", "cxorn", "cxrn", "cxunt", "d0c", "d0k", "d1c", "d1k", "d2c", "d2k", "d3c", "d3k", "d4c", "d4d", "d4k", "d5c", "d5k", "d6c", "d69d", "d6k", "d7c", "d7k", "d8c", "d8k", "d9c", "d9k", "dabi", "dad1", "dad2", "dad3", "dad4", "dad5", "dad6", "dad7", "dad8", "dad9", "dade", "dadi", "dadx", "dady", "date", "datin", "datng", "datx", "datxn", "daxd", "daxe", "daxin", "daxng", "dck", "deku", "denki", "dick", "dicx", "dik", "dirty", "discord", "discxrd", "dixk", "dombf", "domgf", "dpic", "dpxc", "drty", "dscord", "dscrd", "dscxrd", "dude", "duro", "dvdi", "dvdy", "dxad", "dxck", "dxde", "dxdi", "dxdx", "dxdy", "dxk", "dxrt", "dxscord", "dxscrd", "dxscxrd", "dxte", "dxtin", "dxtr", "eater", "eating", "eatng", "eatpus", "eatpux", "eatpxs", "edgin", "f4k", "facesit", "fck", "fem", "fetish", "fivesom", "fivesum", "fker", "fkp", "fkr", "fkxp", "flirt", "flrt", "flxrt", "fme", "foursom", "foursum", "freak", "frnd", "fuck", "fucx", "fuk", "funx", "futa", "fux", "fvck", "fvk", "fvxk", "fxck", "fxk", "fxuc", "g0rl", "g0rn", "g1r1", "g1rl", "gae", "gand", "gay", "gir1", "girl", "givehead", "gixl", "gentlemn", "gentlmn", "gntlemn", "gntlmn", "going2fk", "goingtofk", "gorl", "gorn", "grl", "grxl", "gurl", "guxl", "guy", "gvrl", "gvy", "gw0rl", "gworl", "gxrl", "gxy", "h0ny", "h0rd", "h0rm", "h0rn", "h0rx", "h0t", "h0tdude", "h0xrm", "h0xrn", "h1ny", "h1rd", "h1rn", "h2ny", "h2rd", "h2rn", "h3ny", "h30rm", "h30rn", "h3orm", "h3orn", "h3rd", "h3rn", "h4ny", "h4rd", "h4rn", "h5ny", "h5rd", "h5rn", "h6ny", "h6rd", "h6rn", "h7ny", "h7rd", "h7rn", "h8ny", "h8rd", "h8rn", "h9ny", "h9rd", "h9rn", "handsom", "handsum", "harn", "hauasi", "hausi", "havasi", "havsi", "hawasi", "hawn", "hawsi", "hawx", "helpme", "hirm", "hirn", "hixk", "hmrx", "hmry", "hnr0", "hnri", "hnrx", "hnry", "hnxr", "hnxy", "ho1m", "ho1n", "ho1rm", "ho1rn", "hock", "hokup", "homx", "honx", "hor1n", "horb", "hore", "horm", "horwm", "horwn", "horx", "howrm", "howrn", "hoxi", "hoxm", "hoxn", "hoxrm", "hoxrn", "hoxt", "hoxy", "hpmi", "hpmy", "hpni", "hpny", "hprm", "hprn", "hq0m", "hq0n", "hq1m", "hq1n", "hq2m", "hq2n", "hq3m", "hq3n", "hq4m", "hq4n", "hq5m", "hq5n", "hq6m", "hq6n", "hq7m", "hq7n", "hq8m", "hq8n", "hq9m", "hq9n", "hqrm", "hqrn", "hr0m", "hr0n", "hr0xm", "hr0xn", "hrd", "hrm", "hrn", "hroxn", "hrx0n", "hrxd", "hrxi", "hrxm", "hrxn", "hrxon", "hrxy", "hry", "huck", "hugedk", "huorm", "huorn", "hurn", "huxk", "hvorm", "hvorn", "hvrn", "hworm", "hvrm", "hvrn", "hworm", "hworn", "hwrm", "hwrn", "hx0m", "hx0n", "hx0rm", "hx0rn", "hxck", "hxmy", "hxni", "hxnx", "hxny", "hxom", "hxon", "hxorm", "hxorn", "hxot", "hxr0n", "hxrd", "hxri", "hxrm", "hxrn", "hxron", "hxrx", "hxry", "hzorn", "ieat", "igx", "inch", "instx", "inxc", "inxh", "itis0", "itis1", "itis2", "itis3", "itis4", "itis5", "itis6", "itis7", "itis8", "itis9", "its0", "its1", "its2", "its3", "its4", "its5", "its6", "its7", "its8", "its9", "itz0", "itz1", "itz2", "itz3", "itz4", "itz5", "itz6", "itz7", "itz8", "itz9", "iund", "iust", "ixch", "ixnc", "ixnh", "izuku", "j3rk", "jackin", "jackme", "jaxkin", "jerk", "jerkme", "jerkof", "jork", "juliet", "jxckin", "jxrk", "k1nky", "kamasutra", "kink", "knky", "kream", "krexm", "krxam", "krxm", "kuti", "kxnky", "kxti", "l0v3", "l0ve", "l0vr", "ladka", "ladki", "laid", "latina", "latxn", "lauda", "laude", "ldka", "ldki", "lesb", "lnxd", "longone", "lov3", "lovr", "lumd", "lund", "lusty", "lusxt", "luver", "luvr", "luvxr", "lxdka", "lxdki", "lxmd", "lxnd", "lxtin", "lxver", "lxvr", "lxvxr", "m0ods", "makeme", "male", "mami", "mamx", "masterbate", "masturbate", "mdue", "meandu", "meandyou", "mif", "menyou", "mineta", "misher", "mishim", "misingher", "misinghim", "mlf", "mlxf", "mo0ds", "momy", "moan", "moax", "mota", "moxn", "mude", "muds", "mudx", "mudz", "muthmar", "mxan", "mxlf", "mxf", "mxm", "mxsterbate", "mxsturbate", "myd", "mytoy", "mytxy", "n0ods", "nanga", "nangi", "nangu", "nasty", "naughty", "naugty", "naxe", "ndue", "nedbro", "nedsis", "nexd", "nina", "nino", "niple", "nips", "nked", "nkxe", "no0ds", "nolim", "noty", "nsty", "nude", "nuds", "nudx", "nudz", "nughty", "nutin", "nuxd", "nuxe", "nvde", "nxde", "nxds", "nxdx", "nxed", "nxke", "nxsty", "nxty", "olde", "oldr", "oldx", "olxe", "olxr", "oxbs", "oxde", "oxdr", "p3d0", "p3do", "packind", "packingd", "paglu", "paiyan", "papi", "pati", "pawg", "paxi", "payan", "pdx0", "pdxo", "ped0", "pedo", "penis", "penls", "perv", "pglu", "pgrirl", "pics", "pleasure", "pnish", "puci", "pucs", "pucy", "puhs", "punish", "punixh", "pus3", "pusc", "pusi", "pusy", "puxi", "puxs", "puxy", "pxcs", "pxd0", "pxdo", "pxnish", "pxnixh", "pxglu", "pxr", "pxsc", "pxsy", "pxti", "pxund", "pxzy", "raxd", "rbpoy", "rgpirl", "ride", "rideme", "rideonme", "rmanc", "rmant", "rmnc", "rmnt", "rndi", "rndwa", "romanc", "romant", "romnc", "romnt", "romxnc", "romxnt", "roleplay", "romeo", "roufh", "roufx", "rougx", "rouxh", "roxgh", "rxmanc", "rxmant", "rxmxnc", "rxmxnt", "rxnd", "rxp", "rxugh", "s1t", "s13t", "s1ut", "s3t", "s3x", "s4np", "s9ap", "s9np", "s9xp", "sanp", "seatface", "seduce", "seduct", "segs", "send", "sex", "shlong", "showd", "si3t", "siave", "singl", "sitface", "siton", "siut", "sivt", "sixt", "sl3t", "sl4t", "slave", "slax", "slideit", "slt", "slut", "slux", "slve", "slvt", "slxt", "slxut", "slxv", "slzt", "smoldk", "sn4p", "snap", "snaxp", "sngl", "snowbuny", "snowbxny", "snp", "snxap", "snxp", "snxwbuny", "snxwbxny", "spank", "spaxk", "spnk", "spxnk", "steamy", "stepbro", "stepsis", "stepx", "stpbro", "stpsis", "stpxbro", "stpxsis", "stxpbro", "stxpsis", "subf", "subgf", "submisive", "submisxve", "submxsive", "submxsxve", "suck", "sucmy", "sucx", "suxk", "svlt", "sxank", "sxav", "sxbmisive", "sxbmxsxve", "sxck", "sxlv", "sxngl", "sxut", "sxy", "taik", "tamil", "telugu", "thic", "thresom", "thresum", "throat", "throax", "thrxat", "throxt", "thxroat", "tinydk", "titi", "tits", "tity", "tlts", "toes", "toga", "touchme", "tradx", "traxde", "trde", "trxad", "trxde", "trynanut", "txrade", "txhroat", "txroat", "txti", "txts", "txty", "urmine", "useme", "usemy", "vagina", "veiny", "vergon", "violo", "virgin", "vrgn", "vrgon", "w3t", "wank", "want2fk", "wantb", "wantg", "whited", "whore", "whorx", "whxr", "wifey", "waxt", "wnxt", "wxnt", "wxor", "wxt", "xes", "xfun", "xgram", "xl3t", "xlove", "xlut", "xobs", "xore", "xude", "xunt", "mxster" };

    private static readonly IDaterNameMlModel? NameMLCheck;
    private static Dictionary<string, string>? _diacriticsMap;
    private static readonly ConcurrentDictionary<DaterCacheKey, DaterDetectionResult> DaterResultCache = new();
    private static readonly ConcurrentDictionary<string, bool> RandomAuNameCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, string> DiacriticsResultCache = new(StringComparer.Ordinal);
    private const int MaxDaterCacheEntries = 30000;
    private const int MaxRandomNameCacheEntries = 30000;
    private const int MaxDiacriticsCacheEntries = 30000;

    public static readonly string[] EnglishDictAuWords =
    {
        "ace", "ado", "age", "air", "ant", "apt", "art", "awe", "axe", "bag", "bat", "bay", "bay", "bee", "big", "bin", "bow", "bud", "bug", "bus", "bye", "cab", "can", "car", "cat", "cod", "cos", "cow", "coy", "cub", "cud", "cue", "dam", "day", "den", "dew", "dim", "dot", "due", "due", "dun", "ebb", "egg", "elf", "far", "fax", "fee", "few", "fey", "fin", "fir", "fit", "fly", "fog", "fox", "fun", "fur", "gap", "gen", "gig", "gnu", "gun", "gym", "hay", "hen", "hod", "hue", "ice", "ink", "inn", "jam", "jar", "jet", "jib", "jog", "joy", "key", "key", "kin", "kit", "kop", "lap", "lea", "lid", "lip", "lot", "lug", "map", "mid", "mop", "mud", "net", "net", "new", "nib", "nil", "nth", "oak", "oar", "oil", "one", "one", "ore", "our", "own", "pad", "pan", "pea", "pen", "pie", "pin", "pip", "pit", "pod", "pug", "pun", "pup", "rag", "ray", "ria", "rib", "rug", "saw", "sea", "set", "set", "she", "shy", "spa", "spy", "sty", "sum", "sun", "sup", "tab", "tag", "tan", "tap", "tax", "tea", "tee", "ten", "tie", "tin", "tip", "toy", "tub", "use", "vac", "van", "vet", "wad", "wax", "web", "wig", "wit", "wok", "wry", "yea", "yen", "yon", "zoo", "able", "aged", "agog", "aide", "airy", "ajar", "akin", "ammo", "apex", "arch", "arch", "arty", "ashy", "atom", "auto", "avid", "away", "awed", "baby", "band", "bank", "bark", "barn", "base", "base", "bass", "bass", "bath", "bead", "beam", "bean", "bear", "beef", "bend", "best", "bevy", "bike", "bill", "bine", "blog", "blot", "blue", "blur", "boar", "bold", "bold", "bolt", "book", "boot", "born", "boss", "both", "bowl", "boxy", "brag", "brim", "buff", "bulb", "bump", "bunk", "burr", "busy", "cafe", "cake", "calf", "calm", "cane", "cape", "card", "care", "carp", "cart", "case", "cash", "cask", "cave", "cell", "cent", "chic", "chin", "chip", "chop", "city", "clad", "claw", "clay", "clef", "clip", "clod", "clog", "club", "clue", "coal", "coat", "coda", "code", "coin", "colt", "comb", "cook", "cool", "copy", "cord", "core", "cork", "corn", "cosy", "crab", "crew", "crib", "crop", "crow", "cube", "cult", "curd", "curl", "dame", "damp", "dark", "dart", "dash", "dawn", "dear", "deep", "deer", "deft", "desk", "dhal", "dhow", "dial", "dice", "diet", "disc", "dish", "doer", "doll", "dome", "done", "door", "dove", "dray", "drop", "drum", "dual", "duck", "duct", "dusk", "each", "east", "east", "easy", "echo", "ecru", "edge", "edgy", "envy", "epic", "euro", "even", "ewer", "exam", "exit", "fain", "fair", "fair", "fall", "fare", "farm", "fast", "faun", "fawn", "feet", "fell", "fern", "fife", "file", "film", "fine", "fire", "firm", "fish", "five", "flag", "flat", "flax", "flea", "flex", "flit", "flue", "flux", "foal", "foam", "fond", "font", "food", "fore", "form", "foxy", "free", "fuse", "fuss", "gaff", "gala", "gale", "game", "game", "gamy", "gaol", "gate", "germ", "ghat", "gill", "gilt", "glad", "glue", "goal", "goat", "gold", "gold", "gone", "good", "gram", "grey", "grid", "grub", "gulf", "gull", "gust", "hair", "hale", "half", "half", "hall", "hare", "hazy", "heap", "heat", "herd", "hero", "hewn", "hill", "hind", "hive", "home", "home", "hood", "hoof", "hoop", "hour", "huge", "hunt", "iced", "idea", "inch", "inky", "iron", "item", "jail", "joke", "just", "kame", "keel", "keen", "keep", "kelp", "kerb", "king", "kite", "knee", "knot", "kohl", "lace", "lacy", "lamb", "lamp", "lane", "late", "lava", "lawn", "laze", "lead", "leaf", "lean", "left", "lens", "life", "like", "limb", "line", "link", "lino", "lion", "live", "load", "loaf", "loan", "loch", "loft", "logo", "lone", "long", "look", "loop", "lord", "lost", "loud", "luck", "lure", "lush", "mail", "mall", "mane", "many", "mast", "maze", "meal", "meet", "mega", "menu", "mere", "mews", "mice", "mike", "mild", "mill", "mime", "mind", "mine", "mine", "mini", "mint", "mint", "mire", "mitt", "mole", "mood", "moon", "moor", "more", "moss", "most", "much", "musk", "myth", "name", "nave", "navy", "neap", "near", "neat", "neck", "need", "nest", "news", "next", "nice", "nosh", "note", "noun", "nova", "nowt", "null", "numb", "oast", "odds", "ogee", "once", "only", "open", "open", "oval", "oval", "over", "pace", "page", "pail", "pair", "pall", "palm", "park", "part", "past", "past", "path", "pawl", "peak", "peak", "pear", "peel", "pile", "pill", "pink", "pins", "pith", "pity", "plan", "plot", "plum", "plus", "plus", "poem", "poet", "pony", "pool", "pore", "port", "posh", "pout", "pram", "prey", "prim", "prow", "puce", "pure", "purr", "quay", "quin", "quip", "quiz", "raft", "rail", "rain", "rake", "ramp", "rare", "reed", "rent", "rest", "rich", "rife", "ripe", "rise", "road", "roan", "roof", "rope", "rose", "rose", "rosy", "ruby", "ruff", "rule", "rung", "rust", "safe", "saga", "sage", "sail", "sake", "sale", "salt", "salt", "same", "sand", "sane", "save", "scar", "seal", "seam", "seer", "sett", "shed", "ship", "shoe", "shop", "shot", "show", "side", "sign", "silk", "sine", "sink", "site", "size", "skew", "skip", "slab", "sloe", "slow", "slub", "snap", "snow", "snub", "snug", "sock", "sofa", "soil", "sole", "sole", "solo", "some", "song", "soup", "spam", "span", "spar", "spot", "spry", "stag", "star", "stem", "such", "suet", "sure", "swan", "swap", "tale", "tall", "tame", "tank", "tape", "task", "taut", "taxi", "team", "tear", "teat", "tent", "term", "test", "text", "then", "thud", "tick", "tide", "tidy", "tile", "till", "time", "tiny", "toad", "tofu", "toga", "toil", "tomb", "tour", "town", "trad", "tram", "trap", "tray", "trio", "true", "trug", "tsar", "tube", "tuna", "tune", "turf", "turn", "tusk", "twee", "twig", "twin", "twin", "type", "tyre", "unit", "used", "vase", "vast", "veal", "veil", "very", "vest", "view", "vote", "wail", "wall", "wand", "ward", "warm", "wary", "wasp", "wave", "wavy", "waxy", "week", "weir", "well", "well", "west", "west", "whey", "whim", "whip", "wide", "wild", "wile", "will", "wily", "wind", "wing", "wipe", "wire", "wise", "wise", "wish", "wont", "wont", "wool", "worn", "wove", "wren", "yawl", "yawn", "year", "yoke", "yolk", "zany", "zany", "zing", "ackee", "actor", "acute", "adept", "afoot", "agile", "aglow", "alarm", "album", "alert", "alike", "alive", "alkyl", "alkyl", "alloy", "alone", "alpha", "alpha", "amber", "amber", "ample", "angle", "apple", "apron", "arena", "argon", "arrow", "aside", "astir", "atlas", "attic", "audio", "aunty", "avail", "awake", "award", "aware", "awash", "axial", "azure", "badge", "baggy", "balmy", "barge", "basal", "basic", "basin", "basis", "baths", "baton", "baulk", "beach", "beads", "beady", "beefy", "beery", "beige", "bench", "berry", "bhaji", "bidet", "bijou", "bitty", "blank", "blase", "blaze", "bling", "bliss", "bliss", "block", "bloke", "blond", "blues", "blurb", "board", "bonny", "bonus", "booth", "boric", "bound", "bower", "brake", "brass", "brass", "brave", "break", "bream", "bride", "brief", "briny", "brisk", "broad", "broom", "brown", "brown", "bugle", "built", "bulky", "bumpy", "bunch", "cabin", "cable", "cairn", "calyx", "canny", "canoe", "canto", "caret", "cargo", "chain", "chalk", "charm", "chart", "chary", "chess", "chest", "chewy", "chief", "chief", "chill", "chine", "chive", "choir", "chump", "cinch", "civic", "civil", "claim", "clank", "class", "clear", "clerk", "cliff", "cloak", "clock", "close", "cloth", "cloud", "clove", "clump", "coach", "coast", "cocoa", "combe", "comfy", "comic", "comic", "comma", "conic", "coomb", "copse", "coral", "coral", "corps", "court", "coven", "cover", "crane", "crate", "crisp", "crisp", "croak", "crony", "crowd", "crown", "crumb", "crust", "cubic", "curly", "curve", "daily", "dairy", "dairy", "daisy", "dance", "dazed", "delta", "demob", "denim", "diary", "digit", "diner", "dinky", "disco", "ditch", "diver", "divot", "dizzy", "dodge", "domed", "doubt", "dozen", "draft", "drain", "drama", "drawl", "drawn", "dream", "dress", "dried", "drier", "drill", "drink", "drive", "droll", "drone", "duple", "dusky", "dusty", "eager", "eagle", "early", "eater", "elder", "elect", "elfin", "elite", "email", "envoy", "epoch", "equal", "error", "ether", "ethic", "event", "every", "exact", "extra", "facet", "faint", "famed", "fancy", "farad", "fated", "feast", "fence", "ferny", "ferry", "fever", "fibre", "fiery", "filmy", "final", "finch", "fishy", "fizzy", "flash", "flash", "flask", "fleet", "fleet", "flick", "flies", "flock", "flood", "floor", "flour", "fluid", "fluid", "flush", "flute", "focal", "focus", "foggy", "force", "forge", "forty", "fount", "frame", "frank", "fresh", "front", "frost", "frown", "funny", "furry", "furze", "futon", "fuzzy", "gable", "gamma", "gamut", "gauzy", "gecko", "ghost", "giant", "giant", "giddy", "given", "glace", "glass", "glaze", "gleam", "globe", "glory", "glove", "gluey", "going", "goods", "goody", "gooey", "goose", "gorse", "gouge", "gourd", "grace", "grain", "grand", "grand", "grape", "graph", "grasp", "great", "green", "groat", "group", "grown", "guard", "guest", "guide", "guise", "gummy", "gusty", "hanky", "happy", "hardy", "hasty", "heads", "heaps", "heavy", "hedge", "hefty", "helix", "herby", "hertz", "hewer", "hilly", "hinge", "hobby", "holey", "homey", "honey", "hoppy", "hotel", "humid", "husky", "husky", "hutch", "hyena", "icing", "ideal", "image", "imago", "index", "inner", "ionic", "irons", "ivory", "jacks", "jaggy", "jammy", "jazzy", "jeans", "jelly", "jewel", "jokey", "jolly", "juice", "jumbo", "jumbo", "jumpy", "kazoo", "khaki", "kiosk", "knife", "knurl", "koala", "label", "laird", "large", "larky", "larva", "laser", "lasso", "latex", "lathe", "latte", "layer", "leafy", "leaky", "least", "ledge", "leech", "leggy", "lemon", "lento", "level", "level", "lever", "lilac", "limit", "linen", "liner", "litre", "loads", "loamy", "local", "lofty", "logic", "lolly", "loose", "lorry", "loser", "lotto", "lower", "lucid", "lucky", "lunar", "lunch", "lupin", "lyric", "lyric", "magic", "magic", "major", "malty", "mango", "marly", "marsh", "maser", "match", "matey", "maths", "mauve", "mayor", "mealy", "meaty", "medal", "media", "mercy", "merry", "metal", "metal", "meter", "metre", "micro", "miner", "minty", "misty", "mixed", "mixer", "modal", "model", "model", "molar", "month", "moral", "moral", "motel", "motet", "mothy", "motor", "motor", "motte", "mould", "mouse", "mousy", "mouth", "movie", "muddy", "mulch", "mural", "music", "musty", "muted", "natty", "naval", "navvy", "newel", "newsy", "nifty", "night", "ninja", "noble", "noise", "nomad", "north", "north", "notch", "noted", "novel", "novel", "oaken", "ocean", "olden", "olive", "onion", "onset", "orbit", "order", "other", "outer", "outer", "overt", "owing", "oxide", "ozone", "pacer", "pager", "paint", "pally", "palmy", "panda", "paper", "party", "pasty", "patch", "pause", "peace", "peach", "peaky", "pearl", "pearl", "peaty", "peeve", "pence", "penny", "perch", "perky", "petal", "phone", "photo", "piano", "pilot", "pitch", "pithy", "piton", "place", "plain", "plain", "plane", "plank", "plant", "plumy", "plush", "point", "polar", "polka", "porch", "posse", "pouch", "pound", "pouty", "power", "prank", "prawn", "price", "pride", "prime", "prime", "prior", "prism", "privy", "prize", "prize", "prone", "proof", "proof", "prose", "proud", "pulpy", "pupal", "pupil", "puppy", "puree", "purse", "quark", "quart", "query", "quest", "quick", "quiet", "quill", "quilt", "quirk", "quits", "radar", "radio", "radio", "rainy", "rally", "ranch", "range", "rapid", "raven", "razor", "ready", "recap", "redox", "reedy", "regal", "reign", "relay", "remit", "reply", "resit", "retro", "rhyme", "rider", "ridge", "rifle", "right", "rigid", "rimed", "risky", "river", "roast", "robin", "robot", "rocky", "rooms", "roomy", "roost", "round", "route", "royal", "royal", "ruler", "runic", "rural", "rusty", "sable", "salad", "salon", "sassy", "sated", "satin", "saute", "scale", "scaly", "scant", "scarf", "scent", "scoop", "scope", "scrub", "scuff", "sedge", "senna", "sense", "sepia", "seven", "shade", "shaky", "shale", "shame", "shank", "shape", "shark", "sharp", "sheer", "sheet", "shelf", "shell", "shiny", "shirt", "shoal", "shock", "shore", "short", "shrug", "shtum", "sieve", "sight", "silky", "silty", "sixer", "skate", "skill", "skirl", "slang", "slaty", "sleek", "sleet", "slice", "slide", "slime", "small", "smart", "smelt", "smoke", "smoky", "snack", "snail", "snake", "snare", "sniff", "snore", "snowy", "solar", "solid", "solid", "sonic", "soppy", "sorry", "sound", "sound", "soupy", "south", "south", "space", "spare", "spark", "spate", "spawn", "spear", "spent", "spicy", "spiel", "spike", "spire", "spite", "splay", "spoon", "sport", "spout", "spree", "squad", "stack", "staff", "stage", "staid", "stain", "stair", "stamp", "stand", "stare", "start", "state", "state", "steak", "steam", "steel", "steep", "stern", "stick", "still", "stock", "stock", "stoic", "stone", "stony", "stool", "store", "stork", "storm", "story", "stout", "strap", "straw", "stray", "stuck", "study", "style", "suave", "sugar", "sunny", "sunup", "super", "surge", "swarm", "sweet", "sweet", "swell", "swell", "swift", "swipe", "swish", "sword", "sworn", "syrup", "table", "tacit", "tamer", "tangy", "taper", "tarry", "taste", "tawny", "tenon", "tense", "tense", "tenth", "terms", "terse", "theme", "these", "thief", "third", "thorn", "those", "three", "tiara", "tidal", "tiger", "tight", "tilde", "tiled", "tined", "tinny", "tipsy", "tired", "title", "toast", "today", "token", "tonal", "tonic", "topic", "torch", "torte", "total", "total", "towel", "tower", "trail", "train", "treat", "trial", "tribe", "trice", "trike", "trill", "trout", "truce", "truck", "trunk", "trunk", "truss", "truth", "twain", "tweak", "twine", "twirl", "uncut", "undue", "union", "upper", "urban", "usual", "utter", "vague", "valid", "value", "vegan", "verse", "video", "visit", "vista", "vital", "vocal", "voice", "vowel", "wacky", "wagon", "waist", "washy", "watch", "water", "waxen", "weave", "weber", "weeny", "weird", "whale", "wheat", "whiff", "whole", "whorl", "widow", "width", "wince", "winch", "windy", "wiper", "wispy", "witty", "woody", "wordy", "world", "worth", "wound", "wreck", "wrist", "yacht", "yogic", "young", "youth", "yummy", "zebra", "zippy", "zonal", "ablaze", "access", "acting", "action", "active", "actual", "acuity", "adagio", "adroit", "adverb", "advice", "aerial", "aflame", "afloat", "agency", "airway", "alight", "allied", "allure", "amazed", "amoeba", "amount", "anchor", "annual", "annual", "answer", "apeman", "apical", "arable", "arbour", "arcane", "ardent", "ardour", "armful", "armlet", "armour", "arrant", "artful", "artist", "asleep", "aspect", "asthma", "astral", "astute", "atomic", "august", "auntie", "autumn", "avatar", "badger", "ballet", "banner", "barber", "bardic", "barley", "barrel", "basics", "basket", "bathos", "batten", "battle", "beaded", "beaked", "beaker", "bedbug", "bedsit", "beetle", "belief", "benign", "better", "billow", "binary", "bionic", "biotic", "blazon", "blithe", "blotch", "blouse", "blower", "bluish", "blurry", "bonded", "bonnet", "bonsai", "border", "botany", "bottle", "bounds", "bovine", "breach", "breath", "breeze", "breezy", "brewer", "bridge", "bright", "bronze", "brooch", "bubbly", "bubbly", "bucket", "buckle", "budget", "bumper", "bumper", "bundle", "burger", "burrow", "button", "buzzer", "bygone", "byroad", "cachet", "cactus", "camera", "campus", "canape", "candid", "candle", "canine", "canned", "canopy", "canvas", "carbon", "career", "career", "carpet", "carrot", "carton", "castle", "casual", "catchy", "catnap", "cattle", "causal", "caveat", "caviar", "celery", "cellar", "cement", "centre", "centre", "cereal", "cerise", "chalky", "chance", "chancy", "change", "chatty", "cheery", "cheese", "chilly", "chirpy", "choice", "choice", "choral", "chorus", "chummy", "chunky", "cinder", "cinema", "circle", "circus", "classy", "claves", "clayey", "clever", "clinic", "cloche", "cobweb", "cocoon", "coeval", "coffee", "coffer", "cogent", "collar", "collie", "colour", "column", "comedy", "common", "conger", "conoid", "convex", "cookie", "cooler", "coping", "copper", "copper", "cordon", "corned", "corner", "cosmic", "county", "coupon", "course", "covert", "cowboy", "coyote", "cradle", "craggy", "crayon", "creaky", "credit", "crispy", "crumby", "crunch", "cuboid", "cupola", "curacy", "cursor", "curtsy", "custom", "cyclic", "dainty", "damper", "dapper", "daring", "dative", "dazzle", "debate", "debtor", "decent", "defect", "degree", "deluxe", "demure", "denary", "desert", "desire", "detail", "device", "dexter", "diatom", "dilute", "dimple", "dinghy", "direct", "divide", "divine", "docile", "doctor", "dogged", "doodle", "dotage", "doting", "dotted", "double", "doughy", "dragon", "drapes", "drawer", "dreamy", "dressy", "dulcet", "duplex", "earthy", "earwig", "echoey", "effect", "effort", "eighty", "either", "elated", "eldest", "elfish", "elixir", "embryo", "ending", "energy", "engine", "enough", "enough", "entire", "equine", "eraser", "ermine", "errant", "ersatz", "excise", "excuse", "exempt", "exotic", "expert", "expert", "expiry", "extant", "fabled", "facile", "factor", "fallow", "family", "famous", "farmer", "fecund", "feisty", "feline", "fellow", "fencer", "ferric", "fervid", "fierce", "figure", "filial", "fillip", "finish", "finite", "fiscal", "fitful", "fitted", "flambe", "flaxen", "fleece", "fleecy", "flight", "flinty", "floral", "florid", "flossy", "floury", "flower", "fluent", "fluffy", "fodder", "foible", "folder", "folksy", "forage", "forest", "formal", "former", "fridge", "frieze", "fright", "frilly", "frizzy", "frosty", "frothy", "frozen", "frugal", "funnel", "future", "future", "gabled", "gaffer", "gaiter", "galaxy", "gallon", "galore", "gaming", "gaoler", "garage", "garden", "garlic", "gentle", "gerbil", "gifted", "giggly", "ginger", "girder", "glassy", "glider", "glitzy", "global", "glossy", "glossy", "gloved", "golden", "gopher", "gowned", "grainy", "grassy", "grater", "gratis", "gravel", "grease", "greasy", "greeny", "grilse", "gritty", "groove", "grotto", "ground", "grubby", "grungy", "guitar", "gutter", "hairdo", "haloed", "hamlet", "hammer", "hanger", "hawser", "header", "health", "helper", "hempen", "herbal", "hermit", "heroic", "hiccup", "hinder", "hinged", "homely", "homing", "honest", "hoofed", "hooked", "horsey", "hostel", "hourly", "hubbub", "huddle", "humane", "humble", "humour", "hungry", "hunted", "hunter", "hurray", "hybrid", "hyphen", "iambic", "icicle", "iconic", "iguana", "immune", "inborn", "indoor", "inland", "inmost", "innate", "inrush", "insect", "inside", "inside", "instep", "intact", "intent", "intern", "invite", "inward", "iodine", "ironic", "island", "italic", "jacket", "jagged", "jailer", "jargon", "jaunty", "jingle", "jingly", "jockey", "jocose", "jocund", "jogger", "joggle", "jovial", "joyful", "joyous", "jumble", "jumper", "jungly", "junior", "kennel", "ketone", "kettle", "kilted", "kindly", "kingly", "kirsch", "kitbag", "kitten", "knight", "ladder", "landed", "laptop", "larder", "larval", "latest", "latter", "laurel", "lavish", "lawful", "lawyer", "layman", "leaded", "leaden", "league", "ledger", "legacy", "legend", "legion", "lemony", "lender", "length", "lepton", "lessee", "lesser", "lesson", "lethal", "letter", "liable", "lidded", "likely", "limber", "limpid", "lineal", "linear", "liquid", "lissom", "listed", "litter", "little", "lively", "livery", "living", "living", "lizard", "loaded", "loafer", "locker", "locust", "logger", "lordly", "lounge", "lovely", "loving", "lugger", "lupine", "lustre", "luxury", "madcap", "magnet", "maiden", "maiden", "malted", "mammal", "manful", "manned", "manner", "mantis", "manual", "marble", "margin", "marine", "marked", "market", "maroon", "marshy", "mascot", "massif", "matrix", "matted", "matter", "mature", "meadow", "medial", "median", "medium", "memory", "merest", "meteor", "method", "metric", "mickle", "mickle", "midday", "middle", "middle", "mighty", "milieu", "minded", "minute", "minute", "mirror", "missus", "moated", "mobile", "modern", "modest", "modish", "module", "mohair", "molten", "moment", "mosaic", "motion", "motive", "motive", "motley", "moving", "muckle", "mucous", "muddle", "mulish", "mulled", "mullet", "museum", "mutiny", "mutton", "mutual", "muzzle", "myopia", "myriad", "myriad", "mystic", "mythic", "nachos", "narrow", "nation", "native", "natter", "nature", "nearby", "nether", "nettle", "neuter", "newish", "nimble", "nobody", "normal", "notice", "nought", "number", "object", "oblate", "oblong", "oblong", "occult", "octane", "ocular", "oddity", "offcut", "office", "oldish", "oniony", "online", "onrush", "onside", "onward", "opaque", "opener", "orange", "orange", "origin", "ornate", "orphan", "osprey", "outfit", "owlish", "oxtail", "oxygen", "packed", "packet", "palace", "paltry", "papery", "parade", "parcel", "parody", "parrot", "patchy", "patent", "pathos", "pavane", "peachy", "peaked", "peanut", "pebble", "pebbly", "pedlar", "people", "pepper", "petite", "petrol", "phrase", "picker", "picket", "pickle", "picnic", "pigeon", "pillar", "pillow", "pimple", "pimply", "pincer", "pinion", "piping", "pitted", "placid", "planar", "planet", "plaque", "plenty", "pliant", "plucky", "plumed", "plummy", "plunge", "plural", "plural", "plushy", "pocked", "pocket", "pocket", "poetic", "poetry", "poised", "polite", "pollen", "porous", "postal", "poster", "potato", "potted", "pounce", "powder", "precis", "prefix", "pretty", "pricey", "primal", "profit", "prompt", "proper", "proven", "public", "puddle", "pulley", "pulsar", "punchy", "puppet", "purism", "purist", "purple", "purply", "puzzle", "quaint", "quango", "quasar", "quirky", "rabbit", "racing", "racket", "radial", "radius", "raffia", "raffle", "ragged", "raging", "raglan", "raglan", "ragtag", "raisin", "rammer", "ramrod", "random", "rapper", "raring", "rarity", "rasher", "rating", "ration", "rattle", "ravine", "raving", "reason", "rebate", "recent", "recess", "recipe", "record", "record", "redial", "reform", "regent", "region", "relief", "relish", "remark", "remiss", "remote", "rennet", "rennin", "repair", "report", "rested", "result", "retort", "revamp", "reward", "rhythm", "ribbon", "ridden", "riddle", "ridged", "ripple", "rising", "robust", "rocket", "rodent", "rotary", "rotund", "roving", "rubble", "ruched", "rudder", "rueful", "rugged", "rugger", "rumour", "rumpus", "runway", "russet", "rustic", "rustle", "rutted", "saddle", "saithe", "saline", "salmon", "sample", "sandal", "sateen", "satiny", "saucer", "saving", "sawfly", "scalar", "scalar", "scales", "scarab", "scarce", "scenic", "scheme", "school", "schtum", "scorer", "scrawl", "screen", "script", "scurfy", "season", "seated", "second", "secret", "secret", "secure", "sedate", "seemly", "select", "senior", "sensor", "septet", "serene", "serial", "series", "settee", "setter", "severe", "shaper", "sharer", "sheeny", "shield", "shiner", "shorts", "shovel", "shower", "shrewd", "shrill", "shrimp", "signal", "signal", "signet", "silage", "silent", "silken", "silver", "silver", "simian", "simile", "simper", "simple", "sinewy", "single", "sinter", "sister", "sketch", "slangy", "sledge", "sleepy", "sleety", "sleeve", "sleigh", "slight", "slinky", "slippy", "sluice", "slushy", "smooth", "smudge", "smudgy", "snaggy", "snazzy", "snoopy", "snoozy", "social", "socket", "sodium", "softie", "solemn", "solids", "sonnet", "source", "sparky", "speech", "speedy", "sphere", "sphinx", "spider", "spinet", "spiral", "spiral", "spooky", "sporty", "spotty", "sprain", "sprawl", "spring", "spruce", "sprung", "square", "square", "squash", "squish", "stable", "stagey", "stamen", "staple", "staple", "starch", "starry", "static", "statue", "steady", "steely", "stereo", "stereo", "stocks", "stocky", "stolid", "stormy", "streak", "stride", "string", "stripe", "stripy", "stroll", "strong", "stubby", "studio", "sturdy", "subtle", "suburb", "subway", "sudden", "suffix", "sugary", "sulpha", "summer", "sundry", "sunken", "sunlit", "sunset", "superb", "supine", "supper", "supply", "supply", "surfer", "surtax", "survey", "swampy", "swanky", "sweaty", "switch", "swivel", "sylvan", "symbol", "syntax", "syrupy", "tablet", "taking", "talent", "talker", "tangle", "tanker", "tannic", "target", "tartan", "taster", "tavern", "teacup", "teapot", "teasel", "temper", "tennis", "tester", "tether", "thesis", "thirty", "thrill", "throes", "throne", "ticker", "ticket", "tiddly", "tiered", "tights", "timber", "timely", "tinker", "tinned", "tinted", "tipped", "tipple", "tiptop", "tissue", "titchy", "titled", "tomato", "tracer", "trader", "treaty", "treble", "tremor", "trendy", "tricky", "triple", "troops", "trophy", "trough", "truant", "trusty", "tucker", "tufted", "tundra", "tunnel", "turbid", "turkey", "turtle", "tussle", "twirly", "twisty", "umlaut", "unable", "unborn", "undone", "uneven", "unique", "unlike", "unmade", "unpaid", "unread", "unreal", "unsaid", "unseen", "unsold", "untold", "unused", "unwary", "unworn", "upbeat", "uphill", "upland", "uproar", "uptake", "upward", "upwind", "urbane", "urchin", "urgent", "usable", "useful", "utmost", "valley", "vapour", "varied", "veggie", "veiled", "veined", "velour", "velvet", "verbal", "verity", "vernal", "versed", "vertex", "vessel", "viable", "vinous", "violet", "violin", "visage", "viscid", "visual", "volume", "voyage", "waders", "waggle", "waiter", "waiver", "waking", "wallet", "wallop", "walrus", "wanted", "warble", "warder", "wealth", "wearer", "webbed", "webcam", "wedded", "weevil", "wheezy", "whippy", "wicker", "wifely", "wilful", "window", "winged", "winger", "winner", "winter", "wintry", "witted", "wizard", "wobbly", "wonder", "wonted", "wooded", "woolly", "woolly", "worthy", "wreath", "wrench", "yarrow", "yearly", "yellow", "yonder", "zapper", "zenith", "zigzag", "zigzag", "zircon", "zither", "abiding", "ability", "abiotic", "absence", "account", "acidity", "acrobat", "acrylic", "actress", "actuary", "adamant", "addenda", "address", "advance", "aerated", "aerobic", "affable", "ageless", "airport", "alcopop", "alleged", "amazing", "ambient", "amenity", "amiable", "amusing", "anaemia", "ancient", "angelic", "angling", "angular", "animate", "animism", "aniseed", "annular", "annulus", "anodyne", "antacid", "anthill", "antique", "antique", "antonym", "aplenty", "apology", "apparel", "applied", "apropos", "aquatic", "aqueous", "arbiter", "archaic", "article", "ascetic", "aseptic", "assured", "athlete", "attache", "audible", "aureole", "autocue", "average", "avidity", "awesome", "bagpipe", "balcony", "balloon", "bandsaw", "banquet", "bargain", "baronet", "barrage", "bassist", "battery", "beeline", "belated", "beloved", "bemused", "bequest", "bespoke", "betters", "bicycle", "billion", "binding", "biology", "biscuit", "bismuth", "bivalve", "blanket", "blanket", "blatant", "blessed", "blister", "blogger", "blossom", "blowfly", "blurred", "bonfire", "bookish", "boracic", "boulder", "boxroom", "boycott", "boyhood", "bracket", "bravery", "breaded", "breadth", "breathy", "brimful", "brisket", "bristly", "brittle", "bromide", "brother", "buckram", "bucolic", "budding", "builder", "bulrush", "bulwark", "buoyant", "burning", "bursary", "butcher", "buzzard", "cabaret", "cadence", "cadenza", "caisson", "calends", "calorie", "candied", "cannery", "capable", "capital", "capital", "captain", "caption", "capture", "caravan", "caraway", "carbide", "careful", "carmine", "carnage", "cartoon", "carving", "cashier", "cavalry", "ceiling", "centaur", "central", "centric", "century", "ceramic", "certain", "cession", "chamber", "channel", "chapter", "charity", "charmer", "chatter", "checked", "checker", "chemist", "chevron", "chicane", "chicken", "chimney", "chirrup", "chortle", "chuffed", "civvies", "clarion", "classic", "classic", "clastic", "cleaver", "clement", "climate", "clinker", "cluster", "clutter", "coastal", "coating", "coaxial", "cobbled", "coequal", "cognate", "coldish", "collage", "college", "comical", "commune", "compact", "compact", "company", "compass", "complex", "concave", "concert", "concise", "conduit", "conical", "content", "contest", "control", "convert", "cooking", "coolant", "copious", "copycat", "cordial", "coronet", "correct", "council", "counter", "counter", "country", "courage", "courtly", "crackle", "crawler", "crested", "crimson", "crinkly", "croquet", "crucial", "crumbly", "crunchy", "cryptic", "crystal", "crystal", "culvert", "cunning", "cunning", "cupcake", "curator", "curious", "currant", "current", "curried", "cursive", "cursive", "cursory", "curtain", "cushion", "customs", "cutaway", "cutback", "cutlass", "cutlery", "cutting", "cutting", "cyclist", "dabbler", "dancing", "dappled", "darling", "dashing", "dawning", "deadpan", "decagon", "decided", "decimal", "decimal", "decoder", "defiant", "deltaic", "denizen", "dentist", "dervish", "desktop", "desktop", "dessert", "devoted", "devotee", "diagram", "diamond", "diamond", "dietary", "diffuse", "digital", "dignity", "dioxide", "diploid", "diploma", "display", "distant", "disused", "diurnal", "diverse", "divided", "dolphin", "donnish", "dormant", "doughty", "drachma", "drastic", "draught", "drawing", "dresser", "dribble", "driving", "drought", "drummer", "duality", "ductile", "dungeon", "duopoly", "durable", "dustbin", "dutiful", "dynamic", "dynasty", "earmark", "earnest", "earplug", "earring", "earshot", "earthen", "earthly", "eastern", "easting", "eclipse", "economy", "edaphic", "egghead", "elastic", "elastic", "elderly", "elegant", "elegiac", "ellipse", "elusive", "emerald", "emerald", "eminent", "emirate", "emotive", "empties", "endemic", "endless", "engaged", "enquiry", "ensuing", "epicure", "epigeal", "episode", "epitome", "equable", "equator", "equerry", "erosive", "erudite", "eternal", "ethical", "evasive", "evening", "evident", "exalted", "example", "excited", "exhaust", "exigent", "expanse", "express", "extreme", "factual", "fairing", "fancier", "fantasy", "faraway", "fashion", "feather", "feature", "federal", "feeling", "felspar", "ferrety", "ferrous", "ferrule", "fervent", "festive", "fibrous", "fiction", "fighter", "figment", "filings", "finicky", "fishnet", "fissile", "fission", "fitting", "fixated", "fixture", "flannel", "flavour", "flecked", "fledged", "flighty", "flouncy", "flowery", "fluency", "fluster", "fluvial", "foliage", "foliate", "footing", "footman", "forfeit", "fortune", "forward", "forward", "fragile", "freckly", "freebie", "freeman", "freesia", "freezer", "fretted", "friable", "frilled", "fringed", "frosted", "frowsty", "fulsome", "furcate", "furlong", "furrier", "further", "furtive", "fusible", "fusilli", "gainful", "gallant", "gallery", "gamelan", "garbled", "garnish", "gavotte", "gazette", "gearbox", "general", "genteel", "genuine", "germane", "getaway", "gherkin", "gibbous", "gingery", "giraffe", "girlish", "glaring", "gleeful", "glimmer", "glowing", "gnomish", "goggles", "gorilla", "gradual", "grammar", "grandam", "grandee", "graphic", "grating", "gravity", "greatly", "greyish", "greylag", "gristly", "grocery", "grommet", "grooved", "gryphon", "guarded", "guising", "gushing", "gymnast", "habitat", "hafnium", "halcyon", "halfway", "hallway", "halogen", "halting", "halyard", "handbag", "harbour", "harvest", "heading", "healthy", "hearing", "heating", "helical", "helpful", "helping", "herbage", "heroics", "hexagon", "history", "hitcher", "holdall", "holiday", "holmium", "hominid", "homonym", "honeyed", "hopeful", "horizon", "hotline", "hotness", "hulking", "hunched", "hundred", "hurdler", "hurried", "hydrous", "hygiene", "idyllic", "igneous", "immense", "imprint", "inbuilt", "inexact", "infuser", "ingrown", "initial", "initial", "inkling", "inshore", "instant", "instant", "intense", "interim", "interim", "invader", "inverse", "isohyet", "isthmus", "italics", "jackpot", "jasmine", "jocular", "journal", "journey", "jubilee", "justice", "kenning", "kestrel", "keynote", "kindred", "kindred", "kinetic", "kingdom", "kinsman", "kitchen", "knowing", "knuckle", "knurled", "laconic", "lacquer", "lactose", "lagging", "lambent", "lantern", "largish", "lasting", "lateral", "lattice", "lawsuit", "layette", "leading", "leaflet", "learned", "learner", "leather", "lectern", "legible", "leisure", "lengthy", "lenient", "leonine", "leopard", "lettuce", "lexical", "liberty", "library", "lilting", "lineage", "linkage", "linkman", "lioness", "literal", "lithium", "logging", "logical", "longish", "lottery", "louvred", "lovable", "lowland", "luggage", "lyrical", "machine", "maestro", "magenta", "magenta", "magical", "magnate", "majesty", "maltose", "mammoth", "mammoth", "manners", "mansard", "marbled", "marital", "marquee", "mascara", "massive", "matinee", "matting", "mattock", "maximal", "maximum", "mayoral", "meaning", "meaning", "medical", "meeting", "melodic", "mermaid", "message", "midland", "midweek", "million", "million", "mimetic", "mindful", "mineral", "mineral", "minimal", "minimum", "minster", "missile", "missing", "mission", "mistake", "mixture", "modular", "mollusc", "moneyed", "monitor", "monthly", "moonlit", "moorhen", "morello", "morning", "mottled", "mounted", "mourner", "movable", "muddler", "muffler", "mullion", "musical", "mustard", "mustard", "nankeen", "narwhal", "natural", "nebular", "needful", "neither", "netball", "netting", "network", "newness", "nightly", "nitrous", "nomadic", "nominal", "notable", "noughth", "nuclear", "nursery", "nursing", "nurture", "obesity", "oblique", "obscure", "obvious", "oceanic", "octagon", "octopus", "offbeat", "officer", "offline", "offside", "oilcake", "ominous", "onerous", "ongoing", "onshore", "opening", "opinion", "optimal", "optimum", "opulent", "orbital", "orchard", "ordered", "orderly", "ordinal", "ordinal", "organic", "osmosis", "osmotic", "outdoor", "outline", "outside", "outside", "outsize", "outward", "overall", "overarm", "overlay", "package", "padlock", "pageant", "painter", "paisley", "palaver", "palette", "palmate", "palmtop", "panicle", "paragon", "parking", "parlous", "partial", "passage", "passing", "passive", "pastime", "pasture", "patient", "patient", "pattern", "payable", "peacock", "peckish", "pelagic", "pelisse", "penalty", "pendent", "pending", "penguin", "pension", "peppery", "perfect", "perfume", "persona", "phantom", "philtre", "phonics", "picture", "piebald", "pillbox", "pinched", "pinkish", "piquant", "pitcher", "pitfall", "pivotal", "plaster", "plastic", "plastic", "platoon", "playful", "pleased", "pleated", "plenary", "pliable", "plumber", "plunger", "podcast", "poetess", "pointed", "polemic", "politic", "popcorn", "popular", "portion", "postage", "postbox", "postern", "postman", "potable", "pottage", "pottery", "powdery", "powered", "praline", "prattle", "precise", "prefect", "premier", "present", "present", "prickle", "primary", "process", "product", "profuse", "program", "project", "pronged", "pronoun", "propane", "protean", "protein", "proverb", "proviso", "prudent", "psychic", "puckish", "pumpkin", "purpose", "puzzler", "pyjamas", "pyramid", "pyrites", "quality", "quantum", "quarter", "quavery", "queenly", "quinine", "quorate", "rabbity", "rackety", "radiant", "radical", "raffish", "rafting", "railing", "railman", "railway", "rainbow", "rambler", "ramekin", "rampant", "rarebit", "ratable", "raucous", "rawhide", "readies", "recital", "recount", "recruit", "redhead", "redwing", "referee", "refined", "regards", "regatta", "regency", "regnant", "regular", "related", "relaxed", "reliant", "remorse", "removed", "replete", "reproof", "reptile", "reputed", "respect", "restful", "restive", "rethink", "retired", "retread", "revelry", "revenge", "reverse", "rhombus", "rickety", "rimless", "ringing", "riotous", "riviera", "roaring", "robotic", "rolling", "roseate", "rounded", "rounder", "routine", "routine", "ruffled", "ruinous", "runaway", "rundown", "running", "saddler", "sailing", "salient", "salvage", "sampler", "sapient", "sardine", "saurian", "sausage", "savings", "savoury", "scarlet", "scenery", "scented", "science", "scrappy", "scratch", "scrawny", "screech", "scribal", "sealant", "searing", "seasick", "seaside", "seaward", "seaweed", "section", "secular", "seedbed", "seeming", "segment", "seismic", "sensory", "sensual", "serious", "serried", "servant", "several", "shadowy", "shapely", "shelter", "sheriff", "shivery", "shocker", "showery", "showing", "shrubby", "shudder", "shutter", "sickbay", "sidecar", "sighted", "sightly", "signing", "silvery", "similar", "sincere", "sinless", "sinuous", "sixfold", "sketchy", "skilful", "skilled", "skimmed", "skyline", "skyward", "slatted", "sleeved", "slipper", "slotted", "slowish", "slurred", "sniffle", "sniffly", "snuffly", "snuggly", "society", "soldier", "soluble", "someone", "soprano", "sorghum", "soulful", "spangle", "spangly", "spaniel", "spanner", "sparing", "sparkly", "sparrow", "spartan", "spatial", "speaker", "special", "speckle", "spidery", "spindly", "splashy", "splotch", "spotted", "springy", "spurred", "squally", "squashy", "squidgy", "squiffy", "squishy", "stadium", "standby", "standby", "stapler", "starchy", "starlit", "stately", "station", "stature", "staunch", "stealth", "stellar", "sticker", "stilted", "stoical", "strange", "stratum", "streaky", "stretch", "striker", "strings", "stringy", "striped", "stubbly", "student", "studied", "stylish", "styptic", "subject", "subject", "sublime", "success", "suiting", "sultana", "summary", "summary", "summery", "sunburn", "sundial", "sundown", "sunfish", "sunless", "sunrise", "sunroof", "support", "supreme", "surface", "surface", "surfeit", "surgery", "surmise", "surname", "surplus", "surreal", "swarthy", "swearer", "sweater", "swollen", "synapse", "synonym", "tabular", "tactful", "tactile", "tadpole", "tallish", "tangram", "tantrum", "taxable", "teacher", "telling", "tenable", "tenfold", "tensile", "ternary", "terrace", "terrain", "terrine", "testate", "textile", "textual", "texture", "theatre", "thistle", "thought", "thrifty", "through", "thrower", "thunder", "tideway", "timpani", "titanic", "titular", "toaster", "toccata", "tombola", "tonight", "toothed", "topical", "topmost", "topsoil", "torment", "tornado", "touched", "tourism", "tourist", "tracing", "tracker", "tractor", "trailer", "trainer", "trapeze", "treacly", "tremolo", "triable", "triadic", "tribune", "trickle", "trochee", "trolley", "trophic", "tropism", "trouble", "trouper", "trumpet", "tsunami", "tubular", "tumbler", "tunable", "tuneful", "twelfth", "twiddly", "twilled", "twitchy", "twofold", "typical", "umpteen", "unaided", "unarmed", "unasked", "unaware", "unbound", "unbowed", "uncanny", "undying", "unequal", "unheard", "unicorn", "unifier", "uniform", "uniform", "unitary", "unladen", "unlined", "unmoved", "unnamed", "unpaved", "unready", "untried", "unusual", "unwaged", "upfront", "upright", "upriver", "upstage", "upstate", "upswept", "useable", "utility", "utility", "valiant", "vanilla", "variant", "variety", "various", "vaulted", "vehicle", "velvety", "venison", "verbena", "verbose", "verdant", "verdict", "verdure", "vernier", "version", "vesicle", "vibrant", "victory", "vinegar", "vintage", "vintner", "virtual", "visible", "visitor", "vitamin", "vlogger", "volcano", "voltaic", "voluble", "voucher", "vulpine", "waggish", "wagtail", "wakeful", "walkout", "wallaby", "wanting", "warmish", "warrant", "washing", "waverer", "waxwing", "waxwork", "wayward", "wealthy", "wearing", "weather", "weather", "webbing", "website", "weighty", "welcome", "welcome", "western", "wetsuit", "wheaten", "wheelie", "whisker", "widower", "wildcat", "willing", "willowy", "winning", "winsome", "wishful", "wistful", "witness", "woollen", "working", "working", "worldly", "worsted", "wriggly", "wrinkle", "writing", "wrought", "zealous", "zestful"
    };
    public static readonly HashSet<string> EnglishDictAuWordSet = new(EnglishDictAuWords, StringComparer.Ordinal);

    static DaterCheck()
    {
        try
        {
            if (Directory.Exists("nsfw-model-onnx"))
            {
                NameMLCheck = new DaterNameMLCheck("nsfw-model-onnx");
                //Program.EnqueueLog("Dater name detector model loaded succesfully!");
            }
        }
        catch (Exception ex)
        {
            //Program.EnqueueLog(ex.ToString());
        }
    }

    internal static IDaterNameMlModel? DefaultMlModel => NameMLCheck;

    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        if (DiacriticsResultCache.TryGetValue(text, out var cached))
        {
            return cached;
        }

        Dictionary<string, string> diacritics = _diacriticsMap ??= new(){ 
            {"ª","a"},{"à","a"},{"á","a"},{"â","a"},{"ã","a"},{"ä","a"},{"å","a"},{"ā","a"},{"ă","a"},{"ǎ","a"},{"ɑ","a"},{"α","a"},{"а","a"},{"д","a"},{"ạ","a"},{"ả","a"},{"ấ","a"},{"ầ","a"},{"ẩ","a"},{"ẫ","a"},{"ậ","a"},{"ắ","a"},{"ằ","a"},{"ẳ","a"},{"ẵ","a"},{"ặ","a"},{"⒜","a"},{"ⓐ","a"},{"ａ","a"},{"À","A"},{"Á","A"},{"Â","A"},{"Ã","A"},{"Ä","A"},{"Å","A"},{"Ā","A"},{"Ă","A"},{"Ǎ","A"},{"Α","A"},{"Δ","A"},{"Λ","A"},{"λ","A"},{"А","A"},{"Д","A"},{"Ạ","A"},{"Ả","A"},{"Ấ","A"},{"Ầ","A"},{"Ẩ","A"},{"Ẫ","A"},{"Ậ","A"},{"Ắ","A"},{"Ằ","A"},{"Ẳ","A"},{"Ẵ","A"},{"Ặ","A"},{"Å","A"},{"∆","A"},{"∧","A"},{"⊿","A"},{"⌆","A"},{"⏃","A"},{"⏄","A"},{"⏅","A"},{"Ⓐ","A"},{"▲","A"},{"△","A"},{"★","A"},{"☆","A"},{"⼈","A"},{"⼊","A"},{"⽕","A"},{"ㅅ","A"},{"Ａ","A"},{"♲","A"},{"♳","A"},{"♴","A"},{"♵","A"},{"♶","A"},{"♷","A"},{"♸","A"},{"♹","A"},{"♺","A"},{"♻","A"},{"⚠","A"},{"︽","A"},{"︿","A"},{"♠","A"},{"♣","A"},{"♤","A"},{"♧","A"},
            {"æ","ae"},{"Æ","AE"},
            {"Ъ","b"},{"Ь","b"},{"ъ","b"},{"ь","b"},{"⒝","b"},{"ⓑ","b"},{"♭","b"},{"ß","B"},{"Б","B"},{"В","B"},{"в","B"},{"Ⓑ","B"},
            {"Ы","bl"},{"ы","bi"},
            {"¢","c"},{"©","c"},{"ç","c"},{"с","c"},{"ς","c"},{"⊂","c"},{"⊄","c"},{"⊆","c"},{"⊊","c"},{"⒞","c"},{"ⓒ","c"},{"ㄷ","c"},{"ｃ","c"},{"ﾧ","c"},{"￠","c"},{"㈂","c"},{"㉢","c"},{"ㄸ","cc"},{"ﾨ","cc"},{"Ç","C"},{"С","C"},{"℃","C"},{"Ⓒ","C"},{"⼕","C"},{"⼖","C"},{"⿷","C"},{"【","C"},{"〖","C"},{"ㄈ","C"},{"Ｃ","C"},
            {"đ","d"},{"ð","d"},{"δ","d"},{"₫","d"},{"∂","d"},{"⒟","d"},{"ⓓ","d"},{"ㆳ","d"},{"ｄ","d"},{"♩","d"},{"♪","d"},{"Ð","D"},{"Đ","D"},{"Ⓓ","D"},{"Ｄ","D"},
            {"è","e"},{"é","e"},{"ê","e"},{"ë","e"},{"ē","e"},{"ě","e"},{"ε","e"},{"е","e"},{"ё","e"},{"ẹ","e"},{"ẻ","e"},{"ẽ","e"},{"ế","e"},{"ề","e"},{"ể","e"},{"ễ","e"},{"ệ","e"},{"℮","e"},{"⒠","e"},{"ⓔ","e"},{"⺋","e"},{"⺒","e"},{"ｅ","e"},{"£","E"},{"È","E"},{"É","E"},{"Ê","E"},{"Ë","E"},{"Ē","E"},{"Ě","E"},{"Ε","E"},{"Ξ","E"},{"Σ","E"},{"ξ","E"},{"Ё","E"},{"Е","E"},{"Ẹ","E"},{"Ẻ","E"},{"Ẽ","E"},{"Ế","E"},{"Ề","E"},{"Ể","E"},{"Ễ","E"},{"Ệ","E"},{"€","E"},{"∈","E"},{"∉","E"},{"∊","E"},{"∑","E"},{"Ⓔ","E"},{"モ","E"},{"ㅌ","E"},{"ㆯ","E"},{"㈋","E"},{"㉫","E"},{"Ｅ","E"},{"ミ","E"},
            {"ƒ","f"},{"⒡","f"},{"ⓕ","f"},{"ｆ","f"},{"℉","F"},{"Ⓕ","F"},{"Ｆ","F"},
            {"ﬀ","ff"},{"ﬁ","fi"},{"ﬂ","fl"},{"ﬃ","ffi"},{"ﬄ","ffl"},
            {"ɡ","g"},{"ℊ","g"},{"⒢","g"},{"ⓖ","g"},{"ｇ","g"},{"Ⓖ","G"},{"⼛","G"},{"Ｇ","G"},
            {"ℏ","h"},{"⒣","h"},{"ⓗ","h"},{"ん","h"},{"Η","H"},{"Н","H"},{"н","H"},{"Ⓗ","H"},{"⧺","H"},{"⧻","HH"},
            {"¦","i"},{"¡","i"},{"ì","i"},{"í","i"},{"î","i"},{"ï","i"},{"ĩ","i"},{"ī","i"},{"ι","i"},{"ⅰ","i"},{"↑","i"},{"↕","i"},{"⇧","i"},{"Ⓘ","i"},{"┆","i"},{"┇","i"},{"┊","i"},{"┋","i"},{"╎","i"},{"╏","i"},{"⬆","i"},{"⬇","i"},{"〡","i"},{"㆐","i"},{"㇑","i"},{"︴","i"},{"ｉ","i"},{"￤","i"},{"Ì","I"},{"Í","I"},{"Î","I"},{"Ï","I"},{"Ĩ","I"},{"Ī","I"},{"Ι","I"},{"Ⅰ","I"},{"↓","I"},{"⇩","I"},{"∣","I"},{"⒤","I"},{"ⓘ","I"},{"⼁","I"},{"ㅣ","I"},{"︱","I"},{"︳","I"},{"ￜ","I"},{"Ｉ","I"},
            {"Ⅳ","iv"},{"ⅳ","iv"},{"Ⅵ","vi"},{"ⅵ","vi"},{"Ⅶ","vii"},{"ⅶ","vii"},{"Ⅷ","viii"},{"ⅷ","viii"},{"ⅱ","ii"},{"〢","ii"},{"‼","ii"},{"Ⅱ","II"},{"‖","II"},{"ⅲ","iii"},{"〣","iii"},{"Ⅲ","III"},{"Ю","io"},{"ю","io"},
            {"⒥","j"},{"ⓙ","j"},{"ｊ","j"},{"Ⓙ","J"},{"㆜","J"},{"Ｊ","J"},
            {"⒦","k"},{"ⓚ","k"},{"ｋ","k"},{"Κ","K"},{"κ","K"},{"К","K"},{"к","K"},{"Ⓚ","K"},{"Ｋ","K"},{"㉿","K"},
            {"⒧","l"},{"ⓛ","l"},{"ｌ","l"},{"│","l"},{"┃","l"},{"▎","l"},{"▏","l"},{"⎱","l"},{"Ⓛ","L"},{"Ｌ","L"},{"⎿","L"},{"ㆹ","L"},{"㇄","L"},{"㇏","L"},{"㇗","L"},{"㇙","L"},{"㇜","L"},{"㇟","L"},{"└","L"},{"┕","L"},{"┖","L"},{"┗","L"},{"⻌","L"},{"⻍","L"},{"⻎","L"},{"⿺","L"},{"し","L"},{"じ","L"},{"║","ll"},
            {"ḿ","m"},{"ⓜ","m"},{"ｍ","m"},{"М","M"},{"м","M"},{"Ḿ","M"},{"Ⓜ","M"},{"𝖬","M"},{"Ｍ","M"},
            {"ñ","n"},{"ń","n"},{"ň","n"},{"ǹ","n"},{"η","n"},{"Л","n"},{"П","n"},{"л","n"},{"п","n"},{"ⓝ","n"},{"ｎ","n"},{"⺆","n"},{"⺇","n"},{"⼌","n"},{"⼍","n"},{"⼑","n"},{"⾨","n"},{"⾾","n"},{"⿵","n"},{"れ","n"},{"ㄇ","n"},{"︵","n"},{"︷","n"},{"︹","n"},{"︻","n"},{"﹇","n"},{"Ñ","N"},{"Ń","N"},{"Ň","N"},{"Ǹ","N"},{"Ν","N"},{"И","N"},{"Й","N"},{"и","N"},{"й","N"},{"Ⓝ","N"},{"𝖭","N"},{"Ｎ","N"},{"ℵ","N"},
            {"№","No"},
            {"¤","o"},{"ò","o"},{"ó","o"},{"ô","o"},{"õ","o"},{"ö","o"},{"ø","o"},{"ō","o"},{"ŏ","o"},{"ơ","o"},{"ǒ","o"},{"ο","o"},{"σ","o"},{"φ","o"},{"о","o"},{"ọ","o"},{"ỏ","o"},{"ố","o"},{"ồ","o"},{"ổ","o"},{"ỗ","o"},{"ộ","o"},{"ớ","o"},{"ờ","o"},{"ở","o"},{"ỡ","o"},{"ợ","o"},{"∅","o"},{"⊕","o"},{"⊖","o"},{"⊗","o"},{"⊘","o"},{"⊙","o"},{"⏀","o"},{"⏁","o"},{"⏂","o"},{"⒪","o"},{"■","o"},{"□","o"},{"▢","o"},{"▣","o"},{"▤","o"},{"▥","o"},{"▦","o"},{"▧","o"},{"▨","o"},{"▩","o"},{"▪","o"},{"▫","o"},{"◆","o"},{"◇","o"},{"◈","o"},{"◉","o"},{"◊","o"},{"○","o"},{"◌","o"},{"◍","o"},{"◎","o"},{"●","o"},{"◐","o"},{"◑","o"},{"◒","o"},{"◓","o"},{"◦","o"},{"◯","o"},{"☉","o"},{"☯","o"},{"♡","o"},{"♢","o"},{"♥","o"},{"♦","o"},{"✿","o"},{"❀","o"},{"⦿","o"},{"㆕","o"},{"ㅁ","o"},{"ㅇ","o"},{"㇣","o"},{"ㇿ","o"},{"㈄","o"},{"㈇","o"},{"㉤","o"},{"㉧","o"},{"㋺","o"},{"ｏ","o"},{"ﾷ","o"},{"￮","o"},{"〼","o"},{"Ò","O"},{"Ó","O"},{"Ô","O"},{"Õ","O"},{"Ö","O"},{"Ø","O"},{"Ō","O"},{"Ŏ","O"},{"Ơ","O"},{"Ǒ","O"},{"Θ","O"},{"Ο","O"},{"Φ","O"},{"Ω","O"},{"θ","O"},{"О","O"},{"Ф","O"},{"ф","O"},{"Ọ","O"},{"Ỏ","O"},{"Ố","O"},{"Ồ","O"},{"Ổ","O"},{"Ỗ","O"},{"Ộ","O"},{"Ớ","O"},{"Ờ","O"},{"Ở","O"},{"Ỡ","O"},{"Ợ","O"},{"Ω","O"},{"Ⓞ","O"},{"▇","O"},{"█","O"},{"▉","O"},{"▊","O"},{"▋","O"},{"▱","O"},{"♼","O"},{"♽","O"},{"⚽","O"},{"⚾","O"},{"⼝","O"},{"⼞","O"},{"⿴","O"},{"〇","O"},{"〄","O"},{"Ｏ","O"},{"⓪","0"},{"⓿","0"},{"０","0"},{"ㄖ","0"},
            {"œ","oe"},{"Œ","OE"},
            {"ⓟ","p"},{"ｐ","p"},{"þ","p"},{"Þ","p"},{"ρ","p"},{"р","p"},{"Ⓟ","P"},{"𝖯","P"},{"Ｐ","P"},{"Ρ","P"},{"Р","P"},{"⼙","P"},{"⼫","P"},{"ㄕ","P"},{"ㄗ","P"},{"ㆡ","P"},
            {"ⓠ","q"},{"ｑ","q"},{"Ⓠ","Q"},{"𝖰","Q"},{"Ｑ","Q"},
            {"ⓡ","r"},{"ｒ","r"},{"Γ","r"},{"⎾","r"},{"┌","r"},{"┍","r"},{"┎","r"},{"┏","r"},{"╒","r"},{"╓","r"},{"╔","r"},{"╭","r"},{"⺁","r"},{"⼚","r"},{"⼴","r"},{"⽧","r"},{"⿸","r"},{"「","r"},{"『","r"},{"ㄏ","r"},{"ㄬ","r"},{"ㆷ","r"},{"Ⓡ","R"},{"𝖱","R"},{"Ｒ","R"},{"­","R"},{"®","R"},{"Я","R"},{"я","R"},
            {"ⓢ","s"},{"ｓ","s"},{"$","S"},{"Ⓢ","S"},{"𝖲","S"},{"Ｓ","S"},{"⎰","S"},{"∫","S"},{"∮","S"},{"∾","S"},{"ㄎ","S"},{"ㆶ","S"},{"＄","S"},
            {"∬","ss"},{"∭","sss"},
            {"ⓣ","t"},{"ｔ","t"},{"⺊","t"},{"⼔","t"},{"⼗","t"},{"⼘","t"},{"〸","t"},{"ㆺ","t"},{"ㇶ","t"},{"㈦","t"},{"㈩","t"},{"Ⓣ","T"},{"𝖳","T"},{"Ｔ","T"},{"Τ","T"},{"τ","T"},{"Т","T"},{"т","T"},{"⏇","T"},{"⏉","T"},{"┬","T"},{"┭","T"},{"┮","T"},{"┯","T"},{"┰","T"},{"┱","T"},{"┲","T"},{"┳","T"},{"╤","T"},{"╥","T"},{"╦","T"},{"⺅","T"},{"ィ","T"},{"イ","T"},{"ㄒ","T"},{"ￓ","T"},
            {"〹","tt"},
            {"µ","u"},{"ũ","u"},{"ū","u"},{"ŭ","u"},{"ư","u"},{"ǔ","u"},{"ǖ","u"},{"ǘ","u"},{"ǚ","u"},{"ǜ","u"},{"ц","u"},{"ụ","u"},{"ủ","u"},{"ứ","u"},{"ừ","u"},{"ú","u"},{"ử","u"},{"ữ","u"},{"ự","u"},{"⒰","u"},{"ⓤ","u"},{"㇃","u"},{"ｕ","u"},{"︶","u"},{"︸","u"},{"︺","u"},{"︼","u"},{"﹈","u"},{"υ","u"},{"Ũ","U"},{"Ū","U"},{"Ŭ","U"},{"Ư","U"},{"Ǔ","U"},{"Ǖ","U"},{"Ǘ","U"},{"Ǚ","U"},{"Ǜ","U"},{"Ц","U"},{"Ụ","U"},{"Ủ","U"},{"Ú","U"},{"Ứ","U"},{"Ừ","U"},{"Ử","U"},{"Ữ","U"},{"Ự","U"},{"∪","U"},{"℧","U"},{"Ⓤ","U"},{"⿶","U"},{"ひ","U"},{"び","U"},{"ぴ","U"},{"Ｕ","U"},
            {"˅","v"},{"ⅴ","v"},{"∨","v"},{"⒱","v"},{"ⓥ","v"},{"▼","v"},{"▽","v"},{"✓","v"},{"㇢","v"},{"︾","v"},{"﹀","v"},{"ｖ","v"},{"Ⅴ","V"},{"∀","V"},{"Ⓥ","V"},{"Ｖ","V"},
            {"ⓦ","w"},{"ｗ","w"},{"ω","w"},{"ш","w"},{"щ","w"},{"Ⓦ","W"},{"𝖶","W"},{"Ｗ","W"},{"Ш","W"},{"Щ","W"},{"₩","W"},{"￦","W"},
            {"ⓧ","x"},{"ｘ","x"},{"χ","x"},{"х","x"},{"ⅹ","x"},{"〆","x"},{"〤","x"},{"×","x"},{"Ⓧ","X"},{"𝖷","X"},{"Ｘ","X"},{"Χ","X"},{"Х","X"},{"Ⅹ","X"},{"╳","X"},{"ㄨ","X"},{"ㆫ","X"},
            {"⽹","XX"},{"〷","XX"},{"ⓨ","y"},{"ｙ","y"},{"ý","y"},{"ÿ","y"},{"У","y"},{"у","y"},{"ỳ","y"},{"ỵ","y"},{"ỷ","y"},{"ỹ","y"},{"Ⓨ","Y"},{"𝖸","Y"},{"Ｙ","Y"},{"￥","Y"},{"¥","Y"},{"Ý","Y"},{"Υ","Y"},{"Ỳ","Y"},{"Ỵ","Y"},{"Ỷ","Y"},{"Ỹ","Y"},{"ㄚ","Y"},{"ㆩ","Y"},
            {"ⓩ","z"},{"ｚ","z"},{"Ⓩ","Z"},{"𝖹","Z"},{"Ｚ","Z"},{"Ζ","Z"},{"ζ","Z"},{"⼄","Z"},{"㆚","Z"},{"㇠","Z"},
            {"①","1"},{"⑴","1"},{"⒈","1"},{"⓵","1"},{"❶","1"},{"➀","1"},{"➊","1"},{"１","1"},
            {"②","2"},{"⑵","2"},{"⒉","2"},{"⓶","2"},{"❷","2"},{"➁","2"},{"➋","2"},{"２","2"},
            {"³","3"},{"З","3"},{"Э","3"},{"з","3"},{"э","3"},{"∃","3"},{"∋","3"},{"③","3"},{"⑶","3"},{"⒊","3"},{"⓷","3"},{"❸","3"},{"➂","3"},{"➌","3"},{"⺕","3"},{"⼹","3"},{"る","3"},{"ろ","3"},{"ョ","3"},{"ヨ","3"},{"ヺ","3"},{"ㄋ","3"},{"㇋","3"},{"㇌","3"},{"㇡","3"},{"３","3"},
            {"④","4"},{"⑷","4"},{"⒋","4"},{"⓸","4"},{"❹","4"},{"➃","4"},{"➍","4"},{"４","4"},
            {"⑤","5"},{"⑸","5"},{"⒌","5"},{"⓹","5"},{"❺","5"},{"➄","5"},{"➎","5"},{"５","5"},
            {"⑥","6"},{"⑹","6"},{"⒍","6"},{"⓺","6"},{"❻","6"},{"➅","6"},{"➏","6"},{"６","6"},
            {"⑦","7"},{"⑺","7"},{"⒎","7"},{"⓻","7"},{"❼","7"},{"➆","7"},{"➐","7"},{"７","7"},
            {"⑧","8"},{"⑻","8"},{"⒏","8"},{"⓼","8"},{"❽","8"},{"➇","8"},{"➑","8"},{"８","8"},
            {"⑨","9"},{"⑼","9"},{"⒐","9"},{"⓽","9"},{"❾","9"},{"➈","9"},{"➒","9"},{"９","9"},
            {"＠","@"},{"﹫","@"},{"＾","^"},{"⌅","^"},{"㆟","^"},
            {"℀","ac"},{"℅","co"},{"℡","TEL"},{"™","TM"},{"℻","FAX"},{"ⅸ","ix"},{"Ⅸ","IX"},{"ⅺ","xi"},{"Ⅺ","XI"},{"ⅻ","xii"},{"Ⅻ","XII"},{"㉐","PTE"},{"㋌","Hg"},{"㋍","erg"},{"㋎","eV"},{"㋏","LTD"},{"㍱","hPa"},{"㍲","da"},{"㍳","AU"},{"㍴","bar"},{"㍵","oV"},{"㍶","pc"},{"㍷","dm"},{"㍸","dm2"},{"㍹","dm3"},{"㍺","IU"},{"㎀","pA"},{"㎁","nA"},{"㎂","uA"},{"㎃","mA"},{"㎄","kA"},{"㎅","KB"},{"㎆","MB"},{"㎇","GB"},{"㎈","cal"},{"㎉","kcal"},{"㎊","pF"},{"㎋","nF"},{"㎌","uF"},{"㎍","ug"},{"㎎","mg"},{"㎏","kg"},{"㎐","Hz"},{"㎑","kHz"},{"㎒","MHz"},{"㎓","GHz"},{"㎔","Thz"},{"㎕","ul"},{"㎖","ml"},{"㎗","dl"},{"㎘","kl"},{"㎙","fm"},{"㎚","nm"},{"㎛","um"},{"㎜","mm"},{"㎝","cm"},{"㎞","km"},{"㎟","mm2"},{"㎠","cm2"},{"㎡","m2"},{"㎢","km2"},{"㎣","mm3"},{"㎤","cm3"},{"㎥","m3"},{"㎦","km3"},{"㎧","ms"},{"㎨","ms2"},{"㎩","Pa"},{"㎪","kPa"},{"㎫","MPa"},{"㎬","GPa"},{"㎭","rad"},{"㎮","rads"},{"㎯","rads2"},{"㎰","ps"},{"㎱","ns"},{"㎲","us"},{"㎳","ms"},{"㎴","pV"},{"㎵","nV"},{"㎶","uV"},{"㎷","mV"},{"㎸","kW"},{"㎹","MV"},{"㎺","pW"},{"㎻","nW"},{"㎼","uW"},{"㎽","mW"},{"㎾","kW"},{"㎿","MW"},{"㏀","kO"},{"㏁","MO"},{"㏂","am"},{"㏃","Bq"},{"㏄","cc"},{"㏅","cd"},{"㏆","Ckg"},{"㏇","Co"},{"㏈","dB"},{"㏉","Gy"},{"㏊","ha"},{"㏋","HP"},{"㏌","in"},{"㏍","KK"},{"㏎","KM"},{"㏏","kt"},{"㏐","lm"},{"㏑","ln"},{"㏒","log"},{"㏓","lx"},{"㏔","mb"},{"㏕","mil"},{"㏖","mol"},{"㏗","pH"},{"㏘","pm"},{"㏙","PPM"},{"㏚","PR"},{"㏛","sr"},{"㏜","Sv"},{"㏝","Wb"},{"㏞","Vm"},{"㏟","Am"},{"㏿","gal"},
            {"⑩","10"},{"⑽","10"},{"⒑","10"},{"⓾","10"},{"❿","10"},{"➉","10"},{"➓","10"},{"㉈","10"},{"⑪","11"},{"⑾","11"},{"⒒","11"},{"⓫","11"},{"⑫","12"},{"⑿","12"},{"⒓","12"},{"⓬","12"},{"⑬","13"},{"⒀","13"},{"⒔","13"},{"⓭","13"},{"⑭","14"},{"⒁","14"},{"⒕","14"},{"⓮","14"},{"⑮","15"},{"⒂","15"},{"⒖","15"},{"⓯","15"},{"⑯","16"},{"⒃","16"},{"⒗","16"},{"⓰","16"},{"⑰","17"},{"⒄","17"},{"⒘","17"},{"⓱","17"},{"⑱","18"},{"⒅","18"},{"⒙","18"},{"⓲","18"},{"⑲","19"},{"⒆","19"},{"⒚","19"},{"⓳","19"},{"⑳","20"},{"⒇","20"},{"⒛","20"},{"⓴","20"},{"㉉","20"},{"㉑","21"},{"㉒","22"},{"㉓","23"},{"㉔","24"},{"㉕","25"},{"㉖","26"},{"㉗","27"},{"㉘","28"},{"㉙","29"},{"㉚","30"},{"㉊","30"},{"㉛","31"},{"㉜","32"},{"㉝","33"},{"㉞","34"},{"㉟","35"},{"㊱","36"},{"㊲","37"},{"㊳","38"},{"㊴","39"},{"㊵","40"},{"㉋","40"},{"㊶","41"},{"㊷","42"},{"㊸","43"},{"㊹","44"},{"㊺","45"},{"㊻","46"},{"㊼","47"},{"㊽","48"},{"㊾","49"},{"㊿","50"},{"㉌","50"},{"㉍","60"},{"㉎","70"},{"㉏","80"},
            {"-"," "},{";"," "},{"."," "},{"'"," "},{"\\"," "},{"/"," "},{","," "},{"、"," "},{"；"," "}, {"!"," "} };

        var newText = text;

        foreach (var i in diacritics)
        {
            newText = newText.Replace(i.Key, i.Value);
        }

        if (DiacriticsResultCache.Count > MaxDiacriticsCacheEntries)
        {
            DiacriticsResultCache.Clear();
        }

        DiacriticsResultCache[text] = newText;
        return newText;
    }

    public static bool checkAgainstDict(string str, string substr, string[] dict, string reducedStr)
    {
        foreach (string falseDet in dict)
        {
            if (str.Contains(falseDet)) return false;
        }
        return str.Contains(substr) || reducedStr.Contains(substr);
    }

    public static bool IsRandomAUName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 6)
            return false;

        if (RandomAuNameCache.TryGetValue(name, out var cached))
        {
            return cached;
        }

        string[] sussyParts = { "snap", "aunty", "need", "needful", "sup", "wanting", "wanted", "fun", "honey", "boxy" };

        if (!char.IsUpper(name[0]) || !name.Substring(1).All(char.IsLower)) return false; // Among Us only generates random names with the first letter capitalized

        var englishDictAU = EnglishDictAuWordSet;

        for (int i = 1; i < name.Length; i++)
        {
            string first = name.Substring(0, i);
            string second = name.Substring(i);

            if (sussyParts.Contains(first.ToLower()) && sussyParts.Contains(second))
            {
                CacheRandomAuNameResult(name, false);
                return false;
            }

            if (englishDictAU.Contains(first.ToLower()) && englishDictAU.Contains(second.ToLower()))
            {
                CacheRandomAuNameResult(name, true);
                return true;
            }
        }

        CacheRandomAuNameResult(name, false);
        return false;
    }

    private static void CacheRandomAuNameResult(string name, bool value)
    {
        if (RandomAuNameCache.Count > MaxRandomNameCacheEntries)
        {
            RandomAuNameCache.Clear();
        }

        RandomAuNameCache[name] = value;
    }

    public static bool charIsLower(char c)
    {
        return !char.IsLetter(c) || char.IsLower(c);
    }

    public static bool IsDater(string oldInput, bool useML, int playerCount = 4, GameKeywords? lobbyKeywords = null)
    {
        return GetDaterDetection(oldInput, useML, playerCount, lobbyKeywords).Detected;
    }

    public static DaterDetectionResult GetDaterDetection(string oldInput, bool useML, int playerCount = 4, GameKeywords? lobbyKeywords = null)
    {
        if (string.IsNullOrEmpty(oldInput))
        {
            return DaterDetectionResult.NotDetected;
        }

        GameKeywords keywords = lobbyKeywords ?? GameKeywords.All;
        var cacheKey = new DaterCacheKey(oldInput, useML, playerCount, (int)keywords);
        if (DaterResultCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        DaterDetectionResult result = DaterLanguageDetectors.Resolve(keywords).GetDaterDetection(oldInput, useML, playerCount);

        if (DaterResultCache.Count > MaxDaterCacheEntries)
        {
            DaterResultCache.Clear();
        }

        DaterResultCache[cacheKey] = result;
        return result;
    }

    internal static bool IsDaterCore(string oldInput, bool useML, int playerCount = 4, IDaterNameMlModel? mlModel = null)
    {
        return IsDaterCore(oldInput, useML, playerCount, mlModel, out _);
    }

    internal static bool IsDaterCore(string oldInput, bool useML, int playerCount, IDaterNameMlModel? mlModel, out string evidence)
    {
        evidence = string.Empty;

        static bool Mark(ref string evidenceValue, string hit)
        {
            evidenceValue = hit;
            return true;
        }

        if (string.IsNullOrEmpty(oldInput)) return false;

        string input = oldInput;
        string[] approvedNames = { "tallyboi", "herr tussk", "herr tüssk" };
        if (approvedNames.Contains(input.ToLower())) return false;
        if (chatDaterNames.Contains(input)) return Mark(ref evidence, $"chatDaterName:{input}");
        if (IsRandomAUName(input)) return false;

        if (!string.IsNullOrEmpty(input) && input.All(c => !char.IsLetter(c) || char.IsUpper(c)))
        {
            input = input.ToLower();
        }

        if (input.ToLower().Contains("áçê")) return Mark(ref evidence, "special:áçê");

        // Normalize diacritics before token checks.
        input = RemoveDiacritics(input);

        if (playerCount > 0 && playerCount <= 9)
        {
            string cleaned = input.Replace(" ", "").ToLower();
            string cleaned2 = DuplicateLettersRegex().Replace(cleaned, "$1"); // remove duplicate letters
            string cleanedLeet = NormalizeLeetAscii(cleaned);
            string cleaned2Leet = NormalizeLeetAscii(cleaned2);
            string[] basicDet = { "bull", "look4", "xx", "xnx", "xox", "bbs", "ntr",
                "plxx", "fallin", "budd", "bddy", "bxdd", "rufgn", "rufge", "rufgt",
				"lmme", "kiss" };
            string[] basicDet2 = { "bore", "bang", "cht", "fun", "lokin", "lok4", "fart", "fxrt", "frt", "date", "datin", "master", "want", "ned",
                "toy", "fxn", "sux", "puh", "role", "play", "rxle", "plxy", "roxe", "rlxe", "plax", "ask4", "4you", "4u", "spread", "sprexd", "sprxd", "wnna", "wna", "wana",
                "lovly", "lovxly", "lxvly", "lvely", "lvxly", "lxvx", "drain", "budy", "mster", "mastr", "lelo", "dedo", "baby", "down", "dxwn", "dwxn", "doxn", "show", "leta",
                "wild", "stalk", "cvm", "bnha", "husb", "hubb", "wife", "quen", "leme", "bark", "cute", "mine", "your", "send", "snd", "dih", "wnt",
                "cani", "mayi", "milk", "land", "pink", "friend", "doll", "foru", "foryou", "corn", "loyal", "sent" };
            foreach (var i in basicDet)
            {
                if (cleaned.Contains(i) || cleanedLeet.Contains(i)) return Mark(ref evidence, $"basicDet:{i}");
            }
            foreach (var i in basicDet2)
            {
                if (cleaned2.Contains(i) || cleaned2Leet.Contains(i)) return Mark(ref evidence, $"basicDet2:{i}");
            }

            Match datingTag = DatingTagRegex().Match(cleaned);
            if (!datingTag.Success)
            {
                datingTag = DatingTagRegex().Match(cleanedLeet);
            }
            if (datingTag.Success) return Mark(ref evidence, $"datingTag:{datingTag.Value}");

            Match ageGenderTag = AgeGenderRegex().Match(cleaned);
            if (!ageGenderTag.Success)
            {
                ageGenderTag = AgeGenderRegex().Match(cleanedLeet);
            }
            if (ageGenderTag.Success) return Mark(ref evidence, $"ageGender:{ageGenderTag.Value}");

            Match contactTag = ContactHandleRegex().Match(cleaned);
            if (!contactTag.Success)
            {
                contactTag = ContactHandleRegex().Match(cleanedLeet);
            }
            if (contactTag.Success) return Mark(ref evidence, $"contactHandle:{contactTag.Value}");

            // Check for any 1–2 digit number in the cleaned string
            var numberMatches = NumberRegex().Matches(cleaned);
            if (numberMatches.Count == 1 &&
                int.TryParse(numberMatches[0].Value, out int number) &&
                number >= 11 && number < 50 &&
                numberMatches[0].Value.Length == 2)
            {
                return Mark(ref evidence, $"ageNumber:{numberMatches[0].Value}");
            }
            foreach (Match match in numberMatches)
            {
                if (match.Value.Length == 4 &&
                    int.TryParse(match.Value, out int year) &&
                    year >= 1950 && year <= 2025)
                {
                    return Mark(ref evidence, $"birthYear:{match.Value}");
                }
            }
            if (checkAgainstDict(cleaned, "man", new string[] { "railman", "postman", "mansard", "manner", "linkman", "kinsman", "german", "freeman", "footman", "dormant", "adamant", "manual", "mantis", "manful", "layman", "human", "apeman", "mango", "many", "mane", "manga", "mani", "superman", "spiderman", "batman", "mango" }, cleaned)
                && cleaned.EndsWith("man") || cleaned.StartsWith("man") || cleaned == "man" || input.Contains("Man ") || input.Contains(" Man")) return Mark(ref evidence, "man-tag");

            string[] badNames = { "zade", "gaurav", "gauarv", "garuav", "gaurv", "garuv", "blokebusy", "xmadd", "khalid", "oreo", "jannu", "janvu", "hyderabad", "long", "huge", "big", "b1g", "bxg", "pune", "rehan", "sixnine", "brooklyn", "xnik", "nikx", "niksx", "xsnik", "oceanwave", "dheeraj", "hetvi", "rizz", "arjunnn", "damnn", "jackson", "hero",
                "danny", "dxnn", "daxn", "dnny", "india", "dhexraj", "dhxeraj", "dhxerj", "dhexrj", "aiyana", "txvnxet", "missy", "malcom", "jaden", "brock89", "mard", "ombu", "muah", "mwah", "tellme", "haveme", "yummy", "dangg", "minor", "mxnor", "minxr", "mxnxr", "bounce", "onme", "devv",
                "swallow4me", "swallowforme", "swallow", "v0xvst3nn4", "voxvstenna",
                "looking4gf", "looking4bf", "look4gf", "look4bf", "ineedgf", "ineedbf", "ineedagf", "ineedabf", "needagf", "needabf",
                "hotboy", "hotgirl", "cuteboy", "cutegirl", "sexyboy", "sexygirl", "hereforrp", "r4r", "m4f", "f4m",
                "lesmommy", "h0rny80y", "ho0rnyboy", "h0rnyboi", "hornyboi", "hornyboy", "hxrnyboy", "hornym", "adultsonly", "sexourilia",
                "lookingforgf", "lookingforbf", "lookingforlove", "addsnap", "snapaddme", "addmeinsta", "teleid", "tgid",
                "laprivv", "vrgaxinst", "pauzudobi", "hrnxy16boy", "papihot", "needslt", "snapboy", "bscpvtahot", "tbgfbwaext", "sendbxxbs",
                "애널남", "개꼴려", "천박사진교환", "엄마ntr당함", "여자만들어와", "여자만와", "남자임", "개야", "야한", "토크", "토크방", "욕해", "여성분구", "서울여자", "여자구",
                "cvlonapta", "comeburra", "meloneshot", "passivo", "papaya", "bscvajina", "pn3gr4nd", "pnegran", "penegr", "pnexzan", "tumoren", "morenoo",
                "troco", "troc", "ndes", "voglios", "dotat", "ищупошл", "пошл", "damddy", "chudwalo",
                "tussy", "tighttus", "pighttus", "whiteflu", "chikn", "gigganiga", "nigga", "niga", "azgn", "azgin", "azgndr", "facux", "f4cux", "cojeqlo" };
            foreach (var i in badNames)
            {
                if (cleaned.Contains(i) || cleanedLeet.Contains(i)) return Mark(ref evidence, $"badName:{i}");
            }

            string[] heyStrings = { "hey", "helo", "itsme", "its me", "it is me", "itz me", "it iz me", "wsp", "wsup", "wasup" };
            string noDuplicate = DuplicateLettersRegex().Replace(input, "$1");
            foreach (string st in heyStrings)
            {
                if (noDuplicate.ToLower() == st || noDuplicate.ToLower().StartsWith(st) || noDuplicate.Contains(st.ToUpper()) || noDuplicate.ToLower().Contains(" " + st) ||
                    noDuplicate.Contains(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(st)))
                    return Mark(ref evidence, $"heyString:{st}");
            }

            if (input.TrimEnd().Length % 2 == 0 || input.TrimEnd().Count(c => c == ' ') != (int)(input.Length / 2) || input.TrimEnd().Length == 1)
            {
                string[] sussyLetters = { "m", "f", "b", "g", "d", "l" };
                foreach (string letter in sussyLetters)
                {
                    if (input.ToLower() == letter || input.ToLower().StartsWith(letter + " ") || input.ToLower().EndsWith(" " + letter)) return Mark(ref evidence, $"singleLetter:{letter}");
                    if (input.EndsWith(letter.ToUpper()) && charIsLower(input[input.Length - 2])) return Mark(ref evidence, $"singleLetterCaps:{letter}");
                }
            }
        }

        string[] mayFalsePositive = { "rp", "gf", "bf", "r p", "ass", "hny", "mha", "mina", "abs", "fwb", "ddy", "mmy", "bae", "hoe", "hrn",
            "psy", "fvvb", "urs", "fk", "sx", "yn", "pp", "suk", "tn", "gy", "ht", "nty", "eyp", "hii", "hyy", "fuc", "bra", "add" };
        foreach (string st in mayFalsePositive)
        {
            if (input.ToLower() == st || input.ToLower().StartsWith(st) || input.Contains(st.ToUpper()) || input.ToLower().Contains(" " + st) ||
                input.ToLower().Contains("my" + st) ||
                // Covers merged tokens such as "ur" + marker.
                (st != "yn" && input.ToLower().Contains("ur" + st)) ||
                input.ToLower().Contains("4" + st) ||
                input.ToLower().Contains("for" + st) ||
                input.ToLower().Contains("nid" + st) ||
                input.ToLower().Contains("niid" + st) ||
                input.Contains(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(st)) ||
                ((input.IndexOf(st) + st.Length < input.Length && !charIsLower(input[input.IndexOf(st) + st.Length])) &&
                (input.IndexOf(st) - 1 >= 0 && !charIsLower(input[input.IndexOf(st) - 1])) && input.Contains(st)))
                return Mark(ref evidence, $"mayFalsePositive:{st}");
        }

        // Normalize z->x obfuscation variants.
        input = input.Replace('z', 'x').Replace('Z', 'X');

        if (input.Length >= 3 && (
            ((input.StartsWith('R') && (charIsLower(input[1]) || input[1] == ' ')) || (input.StartsWith('r') && (!charIsLower(input[1]) || input[1] == ' '))) &&
            ((input.EndsWith('P') && (charIsLower(input[input.Length - 2]) || input[input.Length - 2] == ' ')) || (input.EndsWith('p') && (!charIsLower(input[input.Length - 2]) || input[input.Length - 2] == ' '))))
            ) return Mark(ref evidence, "pattern:r...p"); // Filter out names such as R James P

        if (input.Length >= 3 && (
            ((input.StartsWith('P') && (charIsLower(input[1]) || input[1] == ' ')) || (input.StartsWith('p') && (!charIsLower(input[1]) || input[1] == ' '))) &&
            ((input.EndsWith('R') && (charIsLower(input[input.Length - 2]) || input[input.Length - 2] == ' ')) || (input.EndsWith('r') && (!charIsLower(input[input.Length - 2]) || input[input.Length - 2] == ' '))))
            ) return Mark(ref evidence, "pattern:p...r"); // Filter out names such as P James R

        string input_mod = DuplicateLettersRegex().Replace(input, "$1");

        if (input_mod.ToLower() == "he" || input_mod.ToLower().StartsWith("he ") || input_mod.Contains("HE") || input_mod.ToLower().EndsWith(" he") || input_mod.ToLower().Contains(" he ")) return Mark(ref evidence, "pronoun:he");
        if (input_mod.ToLower() == "she" || input_mod.ToLower().StartsWith("she ") || input_mod.Contains("SHE") || input_mod.ToLower().EndsWith(" she") || input_mod.ToLower().Contains(" she ")) return Mark(ref evidence, "pronoun:she");
        // Avoid false positives for normal names that contain "he"/"she" substrings.

        string[] sussyEndings = { "gf", "bf", "af", "s x", "mha", "wha" };
        foreach (string st in sussyEndings)
        {
            if (input_mod.EndsWith(st))
                return Mark(ref evidence, $"ending:{st}");
        }

        string[] sussySeparatedWords = { "dic", "rol", "lun", "bi", "dk", "tit", "gul", "pic", "bj", "pr", "bbs", "ddy", "mmy", "bb", "hick", "text", "t3xt",
            "him", "her", "ig", "sup", "me", "m3", "you", "y0u", "ur", "son", "fk", "coc", "cok", "luv", "luvs", "hi", "hy", "hlo", "sub", "h3", "sh3", "izu", "frd", "sc" };
        foreach (string word in sussySeparatedWords)
        {
            if (word == "him" && playerCount > 9) break;
            if (input_mod.ToLower() == word || input_mod.ToLower().StartsWith(word + " ") ||
                input_mod.ToLower().EndsWith(" " + word) || input_mod.ToLower().Contains(" " + word + " ")) return Mark(ref evidence, $"separatedWord:{word}");
            string capitalWord = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word);
            if (input_mod.Contains(capitalWord))
            {
                int wordIndex = input_mod.IndexOf(capitalWord);
                bool separatedByCaps = input_mod.EndsWith(capitalWord) ||
                    (input_mod.StartsWith(capitalWord) && input_mod.Length > word.Length && !charIsLower(input_mod[word.Length])) ||
                    ((wordIndex - 1) > 0 && charIsLower(input_mod[wordIndex - 1]) && input_mod.Length > (wordIndex + word.Length) && !charIsLower(input_mod[wordIndex + word.Length]));
                if (separatedByCaps) return Mark(ref evidence, $"separatedWordCaps:{word}");
            }
            if (input_mod.Contains(word.ToUpper()))
            {
                int wordIndex = input_mod.IndexOf(word.ToUpper());
                bool separatedByCaps = (input_mod.EndsWith(word.ToUpper()) && (wordIndex - 1) > 0 && charIsLower(input_mod[wordIndex - 1])) ||
                    (input_mod.StartsWith(word.ToUpper()) && input_mod.Length > word.Length && charIsLower(input_mod[word.Length])) ||
                    ((wordIndex - 1) > 0 && charIsLower(input_mod[wordIndex - 1]) && input_mod.Length > (wordIndex + word.Length) && charIsLower(input_mod[wordIndex + word.Length]));
                if (separatedByCaps) return Mark(ref evidence, $"separatedWordUpper:{word}");
            }

            if ((input_mod.IndexOf(word) + word.Length < input_mod.Length && !charIsLower(input_mod[input_mod.IndexOf(word) + word.Length])) &&
                (input_mod.IndexOf(word) - 1 >= 0 && !charIsLower(input_mod[input_mod.IndexOf(word) - 1])) && input_mod.Contains(word)) return Mark(ref evidence, $"separatedWordBoundary:{word}");
        }

        if (input_mod.EndsWith("Gir") || input_mod.EndsWith("GIR") || input_mod.ToLower().EndsWith(" gir")) return Mark(ref evidence, "ending:gir");
        // Handles obfuscations of "slt" using uppercase I.
        if (input_mod.Contains("sIt") || input_mod.Contains("SIt")) return Mark(ref evidence, "variant:sIt");

        input = input.ToLower();
        input_mod = input_mod.ToLower();

        if (!input.EndsWith("carp") && !input.EndsWith("sharp") && !input.EndsWith("chirp") && input.EndsWith("rp")) return Mark(ref evidence, "ending:rp");
        if (!input.EndsWith("afk") && input.EndsWith("fk")) return Mark(ref evidence, "ending:fk");
        if (!input.EndsWith("bardic") && !input.EndsWith("melodic") && !input.EndsWith("nomadic") && !input.EndsWith("triadic") && !input.EndsWith("medic") && input.EndsWith("dic")) return Mark(ref evidence, "ending:dic");

        input_mod = input_mod.Replace(" ", "");

        if (checkAgainstDict(input, "hot", new string[] {"shot", "hotel", "photo", "earshot", "hotline", "hotness" }, input_mod)) return Mark(ref evidence, "dict:hot");
        if (checkAgainstDict(input, "horn", new string[] {"thorn", "horns"}, input_mod)) return Mark(ref evidence, "dict:horn");
        if (checkAgainstDict(input, "lick", new string[] {"flick", "slick"}, input_mod)) return Mark(ref evidence, "dict:lick");
        if (checkAgainstDict(input, "wet", new string[] {"wetsuit"}, input_mod)) return Mark(ref evidence, "dict:wet");
        if (checkAgainstDict(input, "hard", new string[] {"hardy", "orchard"}, input_mod)) return Mark(ref evidence, "dict:hard");
        if (checkAgainstDict(input, "boy", new string[] {"boycott", "boyhood"}, input_mod)) return Mark(ref evidence, "dict:boy");
        if (checkAgainstDict(input, "cock", new string[] {"peacock", "cockpit"}, input_mod)) return Mark(ref evidence, "dict:cock");
        if (checkAgainstDict(input, "boi", new string[] {"cuboid", "boil"}, input_mod)) return Mark(ref evidence, "dict:boi");
        if (checkAgainstDict(input, "wank", new string[] {"swanky"}, input_mod)) return Mark(ref evidence, "dict:wank");
        if (checkAgainstDict(input, "chut", new string[] {"chute", "chutney"}, input_mod)) return Mark(ref evidence, "dict:chut");
        if (checkAgainstDict(input, "insta", new string[] {"instant"}, input_mod)) return Mark(ref evidence, "dict:insta");
        // Keep common non-dating words such as "doom" from matching "dom".
        if (input.Contains("dom") && checkAgainstDict(input, "dom", new string[] {"dome", "domed", "random", "kingdom"}, input_mod)) return Mark(ref evidence, "dict:dom");
        if (checkAgainstDict(input, "rough", new string[] {"trough", "drought", "through", "wrought"}, input_mod)) return Mark(ref evidence, "dict:rough");
        if (checkAgainstDict(input, "butt", new string[] {"button", "butter"}, input_mod)) return Mark(ref evidence, "dict:butt");
        if (checkAgainstDict(input, "love", new string[] {"lovely", "beloved", "gloved", "glove", "clove"}, input_mod)) return Mark(ref evidence, "dict:love");
        if (checkAgainstDict(input, "nood", new string[] {"noodle"}, input_mod)) return Mark(ref evidence, "dict:nood");
        if (checkAgainstDict(input, "talk", new string[] {"stalk"}, input_mod)) return Mark(ref evidence, "dict:talk");
        if (checkAgainstDict(input, "hony", new string[] {"anthony"}, input_mod)) return Mark(ref evidence, "dict:hony");
        if (checkAgainstDict(input, "bada", new string[] {"badal", "badapp"}, input_mod)) return Mark(ref evidence, "dict:bada");
        if (checkAgainstDict(input, "sax", new string[] {"saxophone"}, input_mod)) return Mark(ref evidence, "dict:sax");
        if (checkAgainstDict(input, "hron", new string[] {"throne"}, input_mod)) return Mark(ref evidence, "dict:hron");
        if (checkAgainstDict(input, "kami", new string[] {"kamikaze"}, input_mod)) return Mark(ref evidence, "dict:kami");
        if (checkAgainstDict(input, "desi", new string[] {"design"}, input_mod)) return Mark(ref evidence, "dict:desi");
        if (checkAgainstDict(input, "nake", new string[] {"snake"}, input_mod)) return Mark(ref evidence, "dict:nake");
        if (checkAgainstDict(input, "nkey", new string[] {"monkey", "donkey"}, input_mod)) return Mark(ref evidence, "dict:nkey");
        if (checkAgainstDict(input, "rand", new string[] {"random"}, input_mod)) return Mark(ref evidence, "dict:rand");

        if (input_mod.EndsWith("mom") && !input_mod.EndsWith("urmom")) return Mark(ref evidence, "ending:mom");
        if (input_mod.EndsWith("dad") && !input_mod.EndsWith("mydad")) return Mark(ref evidence, "ending:dad");

        string[] hornyNames1 = { "aaja", "bbg", "b00b", "bbc", "bbw", "bigbbs", "bigpp", "boob", "booooooooy", "boooooooy", "booooooy", "boooooy", "booooy", "boooy", "booty", "booy", "boxxd", "buss", "bussy", "buub", "buxx", "buxxy", "bxss", "bxssy", "bxtt", "bxxb", "bxxed", "bxxx", "cxxk", "d3dd", "d4dd", "d5dd", "daad", "dadd", "daxx", "dddi", "dddy", "dicc", "domme", "dvdd", "dxdd", "dxxd", "dxxk", "fcck", "free use", "freeuse", "g meet", "gaand", "gimme head", "gimmehead", "gmeet", "gogle meet", "gonnafk", "googlemeet", "goon", "gujju", "hoorn", "hoxxy", "hrooxn", "hrxxy", "hubby", "hugebbs", "hugepp", "hxoorn", "hxxn", "hxxr", "hxxrn", "hxxry", "hxxxy", "jackoff", "lulli", "mallu", "maluu", "massive d", "massived", "miss her", "miss him", "missher", "misshim", "missingher", "missinghim", "momm", "need b", "need bro", "need g", "need sis", "needb", "needbro", "needfriend", "needg", "needsis", "nipple", "noods", "nxpple", "oobs", "xxbs", "pssy", "pu c", "pucc", "puss", "puxx", "puzz", "pnss", "pnxx", "pnzz", "pxss", "pxxs", "pxzzy", "rpp", "ruff", "sec c", "secc", "segg", "sendd", "smallbbs", "smalldk", "smallpp", "smolpp", "subbf", "submissive", "submissxve", "submxssive", "submxssxve", "sxbmissive", "sxbmxssxve", "sxrp", "teen plus", "teenplus", "threeesom", "threeesum", "threesom", "threesum", "throbbin", "tinybbs", "tinypp", "ttys", "ur knee", "urknee", "myknee", "my knee", "wannafk", "wanttofk", "xadd", "ponnu", "baath", "baate", "damd" };
        foreach (string name in hornyNames1)
        {
            if (input.Contains(name))
            {
                return Mark(ref evidence, $"horny1:{name}");
            }
        }

        foreach (string name in HornyNames2)
        {
            if (input_mod.Contains(name))
            {
                return Mark(ref evidence, $"horny2:{name}");
            }
        }

        if (useML && mlModel is not null && mlModel.IsLikelyDater(input, playerCount))
        {
            evidence = "ml";
            return true;
        }
        return false;
    }

    public static bool IsDaterChat(string chatMsg, int playerCount, GameKeywords? lobbyKeywords = null)
    {
        return GetDaterChatDetection(chatMsg, playerCount, lobbyKeywords).Detected;
    }

    public static DaterDetectionResult GetDaterChatDetection(string chatMsg, int playerCount, GameKeywords? lobbyKeywords = null)
    {
        if (string.IsNullOrWhiteSpace(chatMsg))
        {
            return DaterDetectionResult.NotDetected;
        }

        chatMsg = RemoveDiacritics(chatMsg);
        chatMsg = chatMsg.ToLower();
        if (chatMsg.Contains(";)")) return DaterDetectionResult.Algorithm("chat-basic", ";)");

        string[] hornyChats = { "wyd", "age", "old", "u?", "you", "borin", "wbu", "wby", "hbu", "hby", "hru", "theme", "b?", "g?", "b or g", "g or b", "t or d", "hyy" };
        foreach (string sussy in hornyChats)
        {
            if (chatMsg.Contains(sussy))
            {
                return DaterDetectionResult.Algorithm("chat-basic", $"chatToken:{sussy}");
            }
        }

        if (DaterLanguageDetectors.Resolve(lobbyKeywords ?? GameKeywords.English) is DaterLanguageDetectorBase detector)
        {
            bool mlDetected = detector.NameModel?.IsLikelyDater(chatMsg, playerCount) ?? DefaultMlModel?.IsLikelyDater(chatMsg, playerCount) ?? false;
            if (mlDetected)
            {
                return DaterDetectionResult.MachineLearning("chat-ml");
            }
        }
        return DaterDetectionResult.NotDetected;
    }

    private readonly record struct DaterCacheKey(string Input, bool UseMl, int PlayerCount, int KeywordsValue);

    private static bool LooksLikeDatingTag(string input)
    {
        return DatingTagRegex().IsMatch(input);
    }

    private static bool LooksLikeAgeGenderTag(string input)
    {
        return AgeGenderRegex().IsMatch(input);
    }

    private static bool LooksLikeContactHandleTag(string input)
    {
        return ContactHandleRegex().IsMatch(input);
    }

    internal static string NormalizeLeetAscii(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text
            .Replace('0', 'o')
            .Replace('1', 'i')
            .Replace('3', 'e')
            .Replace('4', 'a')
            .Replace('5', 's')
            .Replace('7', 't')
            .Replace('@', 'a')
            .Replace('$', 's')
            .Replace('!', 'i');
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();

    [GeneratedRegex(@"(?:m4f|f4m|m4m|f4f|m4a|f4a|mf4a|fm4a|r4r|asl|lfgf|lfbf|lgbf|lggf|fwb|dtf|nsa|ltr)")]
    private static partial Regex DatingTagRegex();

    [GeneratedRegex(@"(?:[mf][1-4][0-9]|[1-4][0-9][mf])")]
    private static partial Regex AgeGenderRegex();

    [GeneratedRegex(@"(?:addsnap|snapadd|snapme|snapchat|instaadd|addinsta|teleid|telegramid|tgid|kikme|lineid|kakaoid|wechatid|wxid|vxid|qqid)")]
    private static partial Regex ContactHandleRegex();

    [GeneratedRegex("(.)\\1+")]
    private static partial Regex DuplicateLettersRegex();
}
