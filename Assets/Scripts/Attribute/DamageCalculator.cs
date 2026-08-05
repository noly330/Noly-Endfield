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
        /// 出伤（攻击方算）：最终攻击力 × 招式倍率（暴击 × 暴伤）。返回原始伤害，未减防御。
        /// </summary>
        //TODO:增伤倍率/脆弱debuff 以后加，现在是纯倍率。
        public static float CalculateRawDamage(CharacterAttribute attacker, float damageMul)
        {
            float raw = attacker.FinalATK * damageMul;
            if (attacker.RollCrit())  //如果暴击
                raw *= attacker.CritDamage;
            return raw;
        }

        /// <summary>
        /// 减免（受击方算）：原始伤害 − 自身防御，保底 MinDamage。
        /// </summary>
        public static float ApplyDefense(float rawDamage, float def)
        {
            return Mathf.Max(rawDamage - def, MinDamage);
        }
    }
}