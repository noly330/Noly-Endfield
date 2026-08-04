using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员基础属性模板（数据驱动，只读）。
    /// 运行时通过 CharacterAttribute 读取，叠加武器/加成。
    /// </summary>
    [CreateAssetMenu(menuName = "Endfield/Attribute/CharacterAttributeData")]
    public class CharacterAttributeData : ScriptableObject
    {
        [field: SerializeField] public float maxHp { get; private set; } 
        [field: SerializeField] public float atk { get; private set; }
        [field: SerializeField] public float def { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float critRate { get; private set; }
        [field: SerializeField, Range(1f, 10f)] public float critDamage { get; private set; } = 1.5f;

#if UNITY_EDITOR
        private void OnValidate()   // 设计组手滑时的护栏
        {
            critRate = Mathf.Clamp01(critRate);
            atk = Mathf.Max(atk, 0f);
            def = Mathf.Max(def, 0f);
            maxHp = Mathf.Max(maxHp, 1f);
        }
#endif
    }
}