namespace Endfield
{
    /// <summary>
    /// buff 运行时实例（每实体一份）：配置 + 剩余时间 + 层数 + 施加者。
    /// 生命周期方法转发给所有效果模块。
    /// </summary>
    public class BuffInstance
    {
        public BuffConfig config;
        public float remainingTime;
        public int stackCount = 1;
        /// <summary>施加者</summary>
        public Character caster;
        /// <summary>是否永久</summary>
        public bool IsPermanent => config != null && config.duration <= 0f;
        /// <summary>是否过期</summary>
        public bool IsExpired => !IsPermanent && remainingTime <= 0f;

        public void ApplyEffects(IBuffTarget target)
        {
            if (config.effects != null)
                foreach (var e in config.effects) e?.OnApply(this, target);
        }

        public void StackChangedEffects(IBuffTarget target, int deltaStack)
        {
            if (config.effects != null)
                foreach (var e in config.effects) e?.OnStackChanged(this, target, deltaStack);
        }

        public void TickEffects(IBuffTarget target, float deltaTime)
        {
            if (config.effects != null)
                foreach (var e in config.effects) e?.OnTick(this, target, deltaTime);
        }

        public void RemoveEffects(IBuffTarget target)
        {
            if (config.effects != null)
                foreach (var e in config.effects) e?.OnRemove(this, target);
        }
    }
}
