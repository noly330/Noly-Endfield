using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 通用战斗数据（全角色）：索敌配置 + 普通攻击。干员专属的技能/连携在子类。
    /// </summary>
    [System.Serializable]
    public class CharacterCombatData
    {
        [field: SerializeField] public CombatSetSO normalAttackData { get; private set; }   // 普通攻击（全角色通用）
        public LayerMask targetMask;    // 敌人所在层
        public float targetRadius;      // 索敌半径
    }
}
