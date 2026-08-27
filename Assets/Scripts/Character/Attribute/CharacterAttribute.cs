using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 运行时角色属性：属性数据的唯一读取入口。
    /// 持有基础模板 + 运行时加成，对外提供最终值。
    /// buff 效果通过 AddXxx 接口写入与回退加成，本类不感知具体效果。
    /// 数据源以后换配表，现在先拿SO使用，本类以外的代码无感知。
    /// </summary>
    public class CharacterAttribute
    {
        private readonly CharacterAttributeData _data;   // 基础数据（内嵌于角色SO，以后可换配表行，形状不变）
        public float weaponBaseATK;    // 武器基础攻击（武器系统以后填）

        // 运行时加成（buff/装备通过 AddXxx 写入与回退，累加叠加）
        private float _atkPercentBonus;
        private float _damageTakenPercentBonus;   // 受伤加深（碎甲用），正值 = 受到伤害增加

        public CharacterAttribute(CharacterAttributeData data)
        {
            _data = data;
        }

        /// <summary>最终攻击力 = (基础攻击力 + 武器基础攻击) × (1 + 百分比攻击加成)</summary>
        public float FinalATK => (_data.atk + weaponBaseATK) * (1f + _atkPercentBonus);
        public float MaxHP => _data.maxHp;   // 以后血量组件用
        public float Def => _data.def;
        public float CritRate => _data.critRate;
        public float CritDamage => _data.critDamage;
        /// <summary>受伤加深（0 = 无；碎甲叠上为正值），TakeDamage 结算乘 (1 + 该值)。</summary>
        public float DamageTakenPercent => _damageTakenPercentBonus;

        /// <summary>暴击判定</summary>
        public bool RollCrit() => Random.value < CritRate;

        #region buff/装备运行时加成接口
        /// <summary>攻击力百分比加成（正增负减），buff 写入/回退。</summary>
        public void AddAtkPercent(float value) => _atkPercentBonus += value;
        /// <summary>受伤加深百分比（正=受到伤害增加，碎甲用），buff 写入/回退。</summary>
        public void AddDamageTakenPercent(float value) => _damageTakenPercentBonus += value;
        #endregion
    }
}
