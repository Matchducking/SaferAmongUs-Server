namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterKoreanLanguageDetector : DaterLanguageDetectorBase
{
    public DaterKoreanLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(
            new[] {
                "연애", "친구", "남친", "여친", "나이", "디엠", "카톡", "카톡아이디", "친추", "만남", "소개팅", "조건만남", "섹파", "번호", "연락", "오픈톡", "텔레", "인스타", "라인", "라인아이디", "섹", "야스", "변남", "변녀", "중3",
                "애널", "개꼴려", "꼴려", "천박", "사진교환", "사진", "교환", "ntr", "여자만", "여자만와", "여자만들어와", "남자임", "intp",
                "야한", "개야", "토크", "토크방", "욕해", "여성분", "여성분구", "서울여자", "여자구", "구함"
            },
            useCore: true,
            mlModel: mlModel)
    {
    }
}
