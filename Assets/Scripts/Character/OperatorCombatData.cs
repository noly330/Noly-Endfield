using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员专属战斗数据：在通用 CharacterCombatData（普通攻击/索敌）之上，
    /// 增加干员专属的技能/连携攻击配置。
    /// </summary>
    [System.Serializable]
    public class OperatorCombatData : CharacterCombatData
    {
        [field: SerializeField] public CombatSetSO skillAttackData { get; private set; }
        [field: SerializeField] public CombatSetSO linkAttackData { get; private set; }
        [field: SerializeField] public float linkCooldown { get; private set; }   // 释放连携后需等待 X 秒才能再次入队
    }
}
