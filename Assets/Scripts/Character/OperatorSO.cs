using UnityEngine;

namespace Endfield
{
    [CreateAssetMenu(menuName = "Endfield/Operator/OperatorSO")]
    public class OperatorSO : CharacterSO
    {
        public int ID{get; private set;}
        [field: SerializeField] public OperatorCombatData combatData { get; private set; }
        [field: SerializeField] public OperatorAttributeData attributeData { get; private set; }
    }
}
