namespace Endfield
{
    /// <summary>
    /// buff 代码目录：所有 buff 的配置都定义在这里（纯 C# 配置，不写死到逻辑里）。
    /// </summary>
    public static class BuffDB
    {
        /// <summary>破防值</summary>
        public static readonly BuffConfig DefenseBreak = new BuffConfig
        {
            buffId = 1,
            duration = 30f,
            maxStacks = 4,
            stackingMode = BuffStackingMode.Stack,
            effects = new IBuffEffect[]
            {
                new DefenseBreakEffect(),
            },
        };

        /// <summary>脆弱</summary>
        public static readonly BuffConfig Vulnerable = new BuffConfig
        {
            buffId = 2,
            duration = 12f,
            maxStacks = 4,
            stackingMode = BuffStackingMode.Replace,
            effects = new IBuffEffect[]
            {
                new StatModifierEffect { stat = StatModifierEffect.StatType.DamageTaken, valuePerLayer = 0.04f },
            },
        };

        /// <summary>流血</summary>
        public static readonly BuffConfig Bleed = new BuffConfig
        {
            buffId = 3,
            duration = 5f,
            maxStacks = 1,
            stackingMode = BuffStackingMode.Stack,
            effects = new IBuffEffect[]
            {
                new DamageOverTimeEffect { damagePerSecond = 10f },
            },
        };

        /// <summary>猛击爆发</summary>
        public static readonly BuffConfig SmashBurst = new BuffConfig
        {
            buffId = 4,
            instant = true,
            effects = new IBuffEffect[]
            {
                new BurstDamageEffect { multiplier = 1.25f },
            },
        };

        /// <summary>碎甲爆发</summary>
        public static readonly BuffConfig SunderBurst = new BuffConfig
        {
            buffId = 5,
            instant = true,
            effects = new IBuffEffect[]
            {
                new BurstDamageEffect { multiplier = 0.2f },
            },
        };
    }
}
