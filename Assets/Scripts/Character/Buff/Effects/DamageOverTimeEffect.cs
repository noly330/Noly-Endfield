namespace Endfield
{
    /// <summary>
    /// DoT 效果（流血/燃烧/中毒）：每帧按 `damagePerSecond × 层数 × dt` 扣血。
    /// 扣血走 TakeDamage（hitName 为空 → 不播受击动画）；caster 记在 instance 上作击杀归属。
    /// </summary>
    public class DamageOverTimeEffect : IBuffEffect
    {
        public float damagePerSecond;

        public void OnApply(BuffInstance instance, IBuffTarget target) { }
        public void OnStackChanged(BuffInstance instance, IBuffTarget target, int deltaStack) { }
        public void OnTick(BuffInstance instance, IBuffTarget target, float deltaTime)
        {
            if (target.Health != null)
                target.Health.TakeDamage(new DamageInfo
                {
                    attacker = instance.caster != null ? instance.caster.transform : null,
                    rawDamage = damagePerSecond * instance.stackCount * deltaTime,
                    damageType = DamageType.Status,   // 状态附着伤害：冲刺不可闪避
                });
        }
        public void OnRemove(BuffInstance instance, IBuffTarget target) { }
    }
}
