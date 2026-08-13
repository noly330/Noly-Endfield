using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endfield
{
    /// <summary>
    /// 队伍输入：把输入系统的按键转成 TeamManager 的操作（切人 Q / 技能 1-4）。
    /// </summary>
    public class TeamInputController : MonoBehaviour
    {
        private void OnEnable()
        {
            if (PlayerInputSystem.Instance == null) return;
            PlayerInputSystem.Instance.SwitchAction.performed += OnSwitch;
            PlayerInputSystem.Instance.Skill1.performed += OnSkill1;
            PlayerInputSystem.Instance.Skill2.performed += OnSkill2;
            PlayerInputSystem.Instance.Skill3.performed += OnSkill3;
            PlayerInputSystem.Instance.Skill4.performed += OnSkill4;
            PlayerInputSystem.Instance.LinkSkill.performed += OnLinkSkill;
        }

        private void OnDisable()
        {
            if (PlayerInputSystem.Instance == null) return;
            PlayerInputSystem.Instance.SwitchAction.performed -= OnSwitch;
            PlayerInputSystem.Instance.Skill1.performed -= OnSkill1;
            PlayerInputSystem.Instance.Skill2.performed -= OnSkill2;
            PlayerInputSystem.Instance.Skill3.performed -= OnSkill3;
            PlayerInputSystem.Instance.Skill4.performed -= OnSkill4;
            PlayerInputSystem.Instance.LinkSkill.performed -= OnLinkSkill;
        }

        private void OnSwitch(InputAction.CallbackContext context)
        {
            TeamManager.Instance.SwitchNext();
        }
        private void OnSkill1(InputAction.CallbackContext _) => TeamManager.Instance.TryCastSkill(1);
        private void OnSkill2(InputAction.CallbackContext _) => TeamManager.Instance.TryCastSkill(2);
        private void OnSkill3(InputAction.CallbackContext _) => TeamManager.Instance.TryCastSkill(3);
        private void OnSkill4(InputAction.CallbackContext _) => TeamManager.Instance.TryCastSkill(4);
        private void OnLinkSkill(InputAction.CallbackContext _) => TeamManager.Instance.TryCastLink();
    }
}
