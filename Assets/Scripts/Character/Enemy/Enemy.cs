namespace Endfield
{

    public class Enemy : Character
    {
        public EnemySO enemySO;

        public override CharacterMovementData MovementData => enemySO.movementData;
        public override CharacterCombatData CombatData => enemySO.combatData;
        protected override CharacterAttributeData AttributeData => enemySO.attributeData;
        public override CharacterAIData AIData => enemySO.AIData;
    }
}
