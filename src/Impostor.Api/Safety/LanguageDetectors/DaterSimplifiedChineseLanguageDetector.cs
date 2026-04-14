namespace Impostor.Api.Safety.LanguageDetectors;

internal sealed class DaterSimplifiedChineseLanguageDetector : DaterLanguageDetectorBase
{
    public DaterSimplifiedChineseLanguageDetector(IDaterNameMlModel? mlModel = null)
        : base(new[] { "对象", "對象", "找对象", "交友", "网恋", "恋爱", "戀愛", "年龄", "年齡", "男友", "女友", "私聊", "私信", "私訊", "微信", "加我", "加v", "加q", "vx", "qq", "line", "賴", "電報", "约", "約", "约吗", "約嗎", "约炮", "約炮", "约p", "约x", "炮友", "砲友", "固炮", "裸聊", "处对象", "處對象", "cp", "cpdd" }, useCore: false, mlModel: mlModel)
    {
    }
}
