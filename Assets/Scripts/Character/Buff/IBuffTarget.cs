namespace Endfield
{
    /// <summary>
    /// buff 效果的目标抽象（DIP：效果只依赖该窄接口，不依赖具体 Character 实现）。
    /// 效果需要更多目标能力时，扩展此接口并在 Character 上实现即可。
    /// </summary>
    public interface IBuffTarget
    {
        CharacterAttribute Attribute { get; }
        BuffManager Buffs { get; }
    }
}
