namespace Endfield
{
    /// <summary>
    /// buff 目标抽象（DIP）：效果只通过该窄接口访问目标，不依赖具体 Character 实现。
    /// Attribute=改数值（增伤/免伤）；Buffs=挂别的 buff；Health=扣血（DoT 用）。
    /// </summary>
    public interface IBuffTarget
    {
        CharacterAttribute Attribute { get; }
        BuffManager Buffs { get; }
        CharacterHealth Health { get; }
    }
}
