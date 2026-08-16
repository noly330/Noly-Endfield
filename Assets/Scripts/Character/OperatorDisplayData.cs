using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员展示数据
    /// </summary>
    [System.Serializable]
    public class OperatorDisplayData
    {
        public string name;                     // 显示名（未配时用 asset 名）
        public Sprite avatar;                          // 头像
        public int starLevel;                         // 星级（显示几颗星）
        [TextArea] public string[] skillDescriptions;  // 技能描述（占位）

    }
}
