using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 单个 buff 的运行时状态（SRP）：持有数据 + 当前层数 + 到期时间 + 效果实例（克隆自 BuffDataSO）。
    /// 生命周期方法转发给所有效果；框架不知道任何具体效果是什么（OCP）。
    /// </summary>
    public class BuffRuntime
    {
        public BuffDataSO data { get; }
        public int stackCount { get; private set; }
        public float endTime { get; private set; }

        private readonly BuffEffect[] _effects;

        public BuffRuntime(BuffDataSO data)
        {
            this.data = data;
            stackCount = 1;
            endTime = data.duration > 0f ? Time.time + data.duration : float.MaxValue;
            _effects = CloneEffects(data.effects);
        }

        /// <summary>叠层（封顶 stackCap），并刷新到期时间。返回实际叠加的层数（0 = 已满未叠上）。</summary>
        public int AddStack(int count)
        {
            int prev = stackCount;
            stackCount = Mathf.Min(stackCount + count, data.stackCap);
            if (data.duration > 0f) endTime = Time.time + data.duration;
            return stackCount - prev;
        }

        public bool IsExpired => Time.time >= endTime;

        /// <summary>重设到期时间（供按层数算持续时间的效果用，如碎甲 12s+层×4s）。</summary>
        public void RefreshDuration(float duration) => endTime = duration > 0f ? Time.time + duration : float.MaxValue;

        #region 生命周期转发
        public void FireApply(IBuffTarget target)
        {
            foreach (var e in _effects) e?.OnApply(target, this);
        }
        public void FireStackChanged(IBuffTarget target, int prevStack, int newStack)
        {
            foreach (var e in _effects) e?.OnStackChanged(target, this, prevStack, newStack);
        }
        public void FireTick(IBuffTarget target)
        {
            foreach (var e in _effects) e?.OnTick(target, this);
        }
        public void FireRemove(IBuffTarget target)
        {
            foreach (var e in _effects) e?.OnRemove(target, this);
        }
        #endregion

        /// <summary>克隆效果实例：每个运行时 buff 持有一份，避免多目标共享同一 BuffDataSO 时效果状态串数据。</summary>
        private static BuffEffect[] CloneEffects(BuffEffect[] source)
        {
            if (source == null) return System.Array.Empty<BuffEffect>();
            var clones = new BuffEffect[source.Length];
            for (int i = 0; i < source.Length; i++)
                clones[i] = source[i]?.Clone();
            return clones;
        }
    }
}
