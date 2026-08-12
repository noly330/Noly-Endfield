using System;

namespace Endfield
{
    /// <summary>
    /// buff 效果抽象基类（框架唯一依赖的抽象，DIP）。
    /// OCP：新效果 = 新建子类挂到 BuffDataSO.effects，框架零改动；
    /// LSP：任意子类可替换基类；
    /// ISP：生命周期钩子均为虚方法空实现，效果只覆写自己用到的，不被强迫实现 Tick 等。
    /// 生命周期由 BuffRuntime 按"是否创建/叠层/每帧/移除"转发。
    /// </summary>
    [Serializable]
    public abstract class BuffEffect
    {
        /// <summary>第一次挂上（0→1 层）</summary>
        public virtual void OnApply(IBuffTarget target, BuffRuntime buff) { }

        /// <summary>叠层变化（prev→new，含 apply 之后的每次加层）</summary>
        public virtual void OnStackChanged(IBuffTarget target, BuffRuntime buff, int prevStack, int newStack) { }

        /// <summary>每帧驱动（给 DoT 等持续效果预留）</summary>
        public virtual void OnTick(IBuffTarget target, BuffRuntime buff) { }

        /// <summary>移除（到期 / 被消耗 / 清空）</summary>
        public virtual void OnRemove(IBuffTarget target, BuffRuntime buff) { }

        /// <summary>浅拷贝效果实例（保留具体子类型与字段），供 BuffRuntime 每个实例持有一份，避免多目标共享串状态。</summary>
        public BuffEffect Clone() => (BuffEffect)MemberwiseClone();
    }
}
