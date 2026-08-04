using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 伤害计算：物理伤害公式的唯一入口。
    /// </summary>
    public static class DamageCalculator
    {
        //TODO:保底伤害以后换成其他方案
        private const float MinDamage = 1f;   // 保底：−防御后最低造成的伤害

        /// <summary>
        /// 物理伤害 = 最终攻击力 × 招式倍率（暴击 × 暴伤）− 敌方防御，保底 MinDamage。
        /// </summary>
        //TODO:增伤倍率/脆弱debuff 以后加，现在是纯倍率。
        public static float CalculatePhysical(CharacterAttribute attacker,
            CharacterAttribute target, float damageMul)
        {
            float raw = attacker.FinalATK * damageMul;
            if (attacker.RollCrit())  //如果暴击
                raw *= attacker.CritDamage;
            float damage = raw - (target != null ? target.Def : 0f);   // 目标没有属性时按防御 0
            return Mathf.Max(damage, MinDamage);
        }
    }
}