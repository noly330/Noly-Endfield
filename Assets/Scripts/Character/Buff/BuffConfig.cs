namespace Endfield
{
    /// <summary>
    /// buff 配置（纯数据，flyweight）：共享、不可变、无运行时状态。
    /// 一个配置描述"这个 buff 是什么"；挂到谁身上由 BuffInstance 承载运行时状态。
    /// </summary>
    public class BuffConfig
    {
        public int buffId;
        public float duration = 10f;      // 持续秒数；<=0 永久
        public int maxStacks = 1;        // 最高层数
        public BuffStackingMode stackingMode = BuffStackingMode.Stack;
        public bool instant;             // 瞬发
        public IBuffEffect[] effects;    // 效果模块（无状态，可共享）
    }

    /// <summary>
    /// 重复添加策略：同 buffId 已存在时怎么处理。
    /// Refresh=刷新时长；Stack=叠层并刷新时长；
    /// Replace=覆盖层数并刷新时长；Ignore=忽略。
    /// </summary>
    public enum BuffStackingMode
    {
        Refresh,
        Stack,
        Replace,
        Ignore,
    }
}
