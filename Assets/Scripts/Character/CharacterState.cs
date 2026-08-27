namespace Endfield
{
    /// <summary>
    /// 角色状态（实体状态属性）：影响受击表现与伤害结算。
    /// Normal=默认；SuperArmor=霸体（受伤但不播受击动画）；
    /// Dodging=闪避（被直接攻击触发完美闪避）；PerfectDodging=完美闪避（无视正常攻击，仍受 DoT）。
    /// 未来可加 SkillBody（技能体：不被普攻打断，只能被技能打断）等。
    /// </summary>
    public enum CharacterState
    {
        Normal,
        SuperArmor,
        Dodging,
        PerfectDodging,
    }
}
