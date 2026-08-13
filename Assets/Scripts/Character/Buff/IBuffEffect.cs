namespace Endfield
{
    /// <summary>
    /// 效果模块接口（无状态 flyweight）：一份效果代码可被无数 BuffConfig 复用。
    /// 效果不存任何每目标状态，只读 instance/target 计算——所以能全局共享。
    /// 生命周期由 BuffInstance 转发：挂上 / 每帧 / 移除。
    /// </summary>
    public interface IBuffEffect
    {
        void OnApply(BuffInstance instance, IBuffTarget target);
        void OnStackChanged(BuffInstance instance, IBuffTarget target, int deltaStack);
        void OnTick(BuffInstance instance, IBuffTarget target, float deltaTime);
        void OnRemove(BuffInstance instance, IBuffTarget target);
    }
}
