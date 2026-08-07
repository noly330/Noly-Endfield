using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员：Character 的具体子类，提供干员专属的 OperatorSO 数据。
    /// </summary>
    public class Operator : Character
    {
        [SerializeField] private OperatorSO operatorSO;

        public override CharacterMovementData MovementData => operatorSO.movementData;
        public override CharacterCombatData CombatData => operatorSO.combatData;
        protected override CharacterAttributeData AttributeData => operatorSO.attributeData;
        public override CharacterAIData AIData => operatorSO.AIData;
    }
}
