using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员：Character 的具体子类，提供干员专属的 OperatorSO 数据。
    /// </summary>
    public class Operator : Character
    {
        [SerializeField] private OperatorSO operatorSO;

        /// <summary>干员数据（队伍/编队等按 OperatorSO 标识干员）。</summary>
        public OperatorSO OperatorData => operatorSO;

        public override CharacterMovementData MovementData => operatorSO.movementData;
        public override CharacterCombatData CombatData => operatorSO.combatData;
        protected override CharacterAttributeData AttributeData => operatorSO.attributeData;
        public override CharacterAIData AIData => operatorSO.AIData;
        public override CombatSetSO SkillAttackData => operatorSO.combatData != null ? operatorSO.combatData.skillAttackData : null;
        public override CombatSetSO LinkAttackData => operatorSO.combatData != null ? operatorSO.combatData.linkAttackData : null;
        public override float LinkCooldown => operatorSO.combatData != null ? operatorSO.combatData.linkCooldown : 0f;
    }
}
