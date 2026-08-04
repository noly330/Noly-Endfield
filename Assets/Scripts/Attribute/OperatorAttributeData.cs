using UnityEngine;

namespace Endfield
{
    [CreateAssetMenu(menuName = "Endfield/Attribute/OperatorAttributeData")]
    public class OperatorAttributeData : CharacterAttributeData
    {
        [field: SerializeField] public float ultEnergyCost;   // 终结技所需能量
    }
}
