using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public bool isDead{get;}
    void TakeDamage(DamageInfo damageInfo);
}

/// <summary>
/// 伤害信息：攻击方只负责"出伤"，防御减免由受击方在 TakeDamage 内自行处理。
/// </summary>
public struct DamageInfo
{
    public Transform attacker;   // 攻击者（击杀归属/仇恨/索敌用）
    public float rawDamage;      // 原始伤害（攻击方已算好：ATK×倍率×暴击×增伤），未减防御
    public string hitName;       // 受击动画名（来自 interactionConfig.hitName）
}
