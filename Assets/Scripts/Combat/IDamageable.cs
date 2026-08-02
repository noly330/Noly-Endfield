using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
}

public struct DamageInfo
{
    public Transform attacter;
    public float damage;
}
