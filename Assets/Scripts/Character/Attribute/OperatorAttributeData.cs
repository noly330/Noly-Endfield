using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员专属属性数据
    /// </summary>
    [System.Serializable]
    public class OperatorAttributeData : CharacterAttributeData
    {
        [field: SerializeField] public float ultEnergyCost { get; private set; }   // 终结技所需能量
    }
}
