using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 通用角色属性数据（可序列化数据类，内嵌在角色 SO 中）。
    /// 干员与敌人共用；运行时通过 CharacterAttribute 读取，数据源以后可换配表。
    /// </summary>
    [System.Serializable]
    public class CharacterAttributeData
    {
        [field: SerializeField] public float maxHp { get; private set; }   // 生命上限
        [field: SerializeField] public float atk { get; private set; }     // 干员基础攻击力
        [field: SerializeField] public float def { get; private set; }     // 防御力
        [field: SerializeField, Range(0f, 1f)] public float critRate { get; private set; }      // 暴击率 0~1
        [field: SerializeField, Range(1f, 10f)] public float critDamage { get; private set; } = 1.5f;  // 暴击伤害倍率
    }
}