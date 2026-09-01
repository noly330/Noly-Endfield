using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public bool isDead{get;}
    void TakeDamage(DamageInfo damageInfo);
}

/// <summary>
/// 伤害类型：Direct=直接攻击（实体命中，冲刺可闪避）；Status=状态附着伤害（DoT 流血/燃烧，不可闪避）。
/// </summary>
public enum DamageType
{
    Direct,
    Status,
}

/// <summary>
/// 伤害信息：攻击方只负责"出伤"，防御减免由受击方在 TakeDamage 内自行处理。
/// </summary>
public struct DamageInfo
{
    public Transform attacker;   // 攻击者
    public float rawDamage;      // 原始伤害（攻击方已算好：ATK×倍率×暴击×增伤）
    public string hitName;       // 受击动画名
    public CombatAttackEffectType attackEffectType; // 攻击效果类型
    public DamageType damageType; // 伤害类型（Direct=可闪避；Status=DoT 不可闪避）
    public bool isCrit;          // 是否暴击（飘字用）
}
