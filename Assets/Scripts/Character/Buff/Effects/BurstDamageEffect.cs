namespace Endfield
{
    /// <summary>
    /// 爆发伤害效果
    /// </summary>
    public class BurstDamageEffect : IBuffEffect
    {
        public float multiplier;   // 猛击 1.25 / 碎甲 0.2

        public void OnApply(BuffInstance instance, IBuffTarget target)
        {
            if (instance.caster != null && target.Health != null)
            {
                target.Health.TakeDamage(new DamageInfo
                {
                    attacker = instance.caster.transform,
                    rawDamage = instance.caster.attribute.FinalATK * instance.stackCount * multiplier,
                });
            }
        }
        public void OnStackChanged(BuffInstance instance, IBuffTarget target, int deltaStack) { }
        public void OnTick(BuffInstance instance, IBuffTarget target, float deltaTime) { }
        public void OnRemove(BuffInstance instance, IBuffTarget target) { }
    }
}
