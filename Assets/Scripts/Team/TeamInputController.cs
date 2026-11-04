using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 队伍输入：把输入源的事件转成 TeamManager 的操作（切人 Q / 技能 1-4 / 连携）。
    /// </summary>
    public class TeamInputController : MonoBehaviour
    {
        private IInputSource _input;

        private void OnEnable()
        {
            _input ??= InputService.Current;
            if (_input == null) return;
            _input.OnSwitch += OnSwitch;
            _input.OnSkill += OnSkill;
            _input.OnLinkSkill += OnLinkSkill;
        }

        private void OnDisable()
        {
            if (_input == null) return;
            _input.OnSwitch -= OnSwitch;
            _input.OnSkill -= OnSkill;
            _input.OnLinkSkill -= OnLinkSkill;
        }

        private void OnSwitch() =>TeamManager.Instance.SwitchNext();
        private void OnSkill(int index) => TeamManager.Instance.TryCastSkill(index);
        private void OnLinkSkill() => TeamManager.Instance.Links.TryCastLink();
    }
}
