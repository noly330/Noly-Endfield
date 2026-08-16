using UnityEngine;

namespace Endfield
{
    [CreateAssetMenu(menuName = "Endfield/Operator/OperatorSO")]
    public class OperatorSO : CharacterSO
    {
        [field: SerializeField] public OperatorCombatData combatData { get; private set; }
        [field: SerializeField] public OperatorAttributeData attributeData { get; private set; }
        [field: SerializeField] public OperatorDisplayData displayData { get; private set; }   // 展示数据（名字/头像/技能描述/稀有度）
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public string prefabAddress { get; private set; }
        [field: SerializeField] public LinkTriggerType linkTriggerType { get; private set; }   // 连携触发类型（数据驱动）
    }
}
