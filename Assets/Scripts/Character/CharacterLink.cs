using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 连携触发协调器（挂干员 prefab）：按 OperatorSO.linkTriggerType 订阅对应战斗事件，
    /// 触发时若连携 CD 已好则入队。Character/Operator 保持纯数据/战斗逻辑，不碰全局事件。
    /// </summary>
    public class CharacterLink : MonoBehaviour
    {
        private Operator _operator;

        private void Awake()
        {
            _operator = GetComponent<Operator>();
        }

        private void OnEnable()
        {
            if (_operator == null || _operator.OperatorData == null) return;
            switch (_operator.OperatorData.linkTriggerType)
            {
                case LinkTriggerType.OnDefenseBreakApplied:
                    EventCenter.SubscribeListener<Events.OnDefenseBreakApplied>(OnLinkEvent);
                    break;
                case LinkTriggerType.OnDefenseBreakConsumed:
                    EventCenter.SubscribeListener<Events.OnDefenseBreakConsumed>(OnLinkEvent);
                    break;
                case LinkTriggerType.OnLinkSkillTriggered:
                    EventCenter.SubscribeListener<Events.OnLinkSkillTriggered>(OnLinkEvent);
                    break;
            }
        }

        private void OnDisable()
        {
            if (_operator == null || _operator.OperatorData == null) return;
            switch (_operator.OperatorData.linkTriggerType)
            {
                case LinkTriggerType.OnDefenseBreakApplied:
                    EventCenter.UnsubscribeListener<Events.OnDefenseBreakApplied>(OnLinkEvent);
                    break;
                case LinkTriggerType.OnDefenseBreakConsumed:
                    EventCenter.UnsubscribeListener<Events.OnDefenseBreakConsumed>(OnLinkEvent);
                    break;
                case LinkTriggerType.OnLinkSkillTriggered:
                    EventCenter.UnsubscribeListener<Events.OnLinkSkillTriggered>(OnLinkEvent);
                    break;
            }
        }

        private void OnLinkEvent<T>(T message) where T : struct
        {
            if (_operator == null) return;
            TeamManager.Instance.TryEnqueueLinkAttack(_operator);
        }
    }
}
