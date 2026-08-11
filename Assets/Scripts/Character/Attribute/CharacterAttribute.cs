using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 运行时角色属性：属性数据的唯一读取入口。
    /// 持有基础模板 + 运行时加成，对外提供最终值。
    /// 数据源以后换配表，现在先拿SO使用，本类以外的代码无感知。
    /// </summary>
    public class CharacterAttribute
    {
        private readonly CharacterAttributeData _data;   // 基础数据（内嵌于角色SO，以后可换配表行，形状不变）
        public float weaponBaseATK;    // 武器基础攻击，默认 0，武器系统以后填
        public float percentATKBonus;  // 百分比攻击加成（buff/装备累加）

        public CharacterAttribute(CharacterAttributeData data)
        {
            _data = data;
        }

        /// <summary>最终攻击力 = 干员基础 + 武器基础 × (1 + 百分比攻击加成)</summary>
        public float FinalATK => (_data.atk + weaponBaseATK) * (1f + percentATKBonus);
        public float MaxHP => _data.maxHp;   // 以后血量组件用
        public float Def => _data.def;
        public float CritRate => _data.critRate;
        public float CritDamage => _data.critDamage;

        /// <summary>暴击判定</summary>
        public bool RollCrit() => Random.value < CritRate;
    }
}