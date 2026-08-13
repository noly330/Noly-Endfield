namespace Endfield
{
    /// <summary>
    /// buff 代码目录：所有 buff 的配置都定义在这里（纯 C# 配置，不写死到逻辑里）。
    /// 第 3 步会加破防（Buff_DefenseBreak 配置）。
    /// </summary>
    public static class BuffDB
    {
        /// <summary>脆弱：受伤加深 0.04/层，Replace 按消耗破防层数覆盖（碎甲用）。</summary>
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

        /// <summary>流血：每秒 10 伤 × 层数，演示 DoT。</summary>
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
    }
}
