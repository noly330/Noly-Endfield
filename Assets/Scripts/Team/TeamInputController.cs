using UnityEngine;
using UnityEngine.InputSystem;

namespace Endfield
{
    /// <summary>
    /// 队伍输入：把输入系统的按键转成 TeamManager 的操作。
    /// 当前只接"切人"键（Q，顺序切）。Skill1-4 为技能占位，技能系统接入后再路由。
    /// </summary>
    public class TeamInputController : MonoBehaviour
    {
        private void OnEnable()
        {
            if (PlayerInputSystem.Instance == null) return;
            PlayerInputSystem.Instance.SwitchAction.performed += OnSwitch;
        }

        private void OnDisable()
        {
            if (PlayerInputSystem.Instance == null) return;
            PlayerInputSystem.Instance.SwitchAction.performed -= OnSwitch;
        }

        private void OnSwitch(InputAction.CallbackContext context)
        {
            if (TeamManager.Instance != null)
                TeamManager.Instance.SwitchNext();
        }
    }
}
