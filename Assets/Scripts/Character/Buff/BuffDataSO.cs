using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// buff 数据（纯数据容器，SRP）：描述一个 buff 的标识、时长、叠层与多态效果列表。
    /// effects 用 [SerializeReference] 多态序列化，Inspector 里可为每个元素选择具体 BuffEffect 子类。
    /// </summary>
    [CreateAssetMenu(menuName = "Endfield/Buff/BuffData")]
    public class BuffDataSO : ScriptableObject
    {
        public int buffId;                  // BuffManager 字典键（同一 buff 的唯一标识）
        public string buffName;
        public float duration = 5f;         // 持续时间（秒）；<=0 表示永久（如破防值）
        public int stackCap = 1;            // 叠层上限（破防值上限 = 该值）
        public float valuePerLayer = 1f;    // 每层通用数值（各效果按需解读）
        [SerializeReference] public BuffEffect[] effects;   // 多态效果：新效果 = 新建子类，框架零改动（OCP）
    }
}
