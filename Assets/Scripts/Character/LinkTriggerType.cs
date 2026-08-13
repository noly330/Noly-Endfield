namespace Endfield
{
    /// <summary>
    /// 连携触发类型（配置在 OperatorSO，由 CharacterLink 订阅对应事件）。
    /// OnDefenseBreakApplied  = 敌人破防值增加时入队（连携起点）
    /// OnDefenseBreakConsumed = 敌人破防值被消耗时入队
    /// OnLinkSkillTriggered   = 任意连携打出时入队（连携链）
    /// </summary>
    public enum LinkTriggerType
    {
        None,
        OnDefenseBreakApplied,
        OnDefenseBreakConsumed,
        OnLinkSkillTriggered,
    }
}
