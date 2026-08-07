using UnityEngine;

namespace Endfield
{
    [CreateAssetMenu(menuName = "Endfield/Operator/OperatorSO")]
    public class OperatorSO : CharacterSO
    {
        [field: SerializeField] public OperatorCombatData combatData { get; private set; }
        [field: SerializeField] public OperatorAttributeData attributeData { get; private set; }
    }
}
