using System;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 输入源抽象：玩法/UI 代码只依赖本接口，不感知输入来自本机硬件、AI 还是网络。
    /// 本机实现见 PlayerInputSystem；经 InputService.Current 获取当前输入源。
    /// 事件为"按下瞬间"语义（对应 InputAction.performed）。
    /// </summary>
    public interface IInputSource
    {
        #region 轮询型（每帧读取）
        Vector2 Move { get; }
        Vector2 Look { get; }
        Vector2 Scroll { get; }
        #endregion

        #region 角色控制事件
        event Action OnDash;
        event Action OnAttack;
        #endregion

        #region 队伍事件
        event Action OnSwitch;
        /// <summary>技能 1-4（队伍索引，1 基）。</summary>
        event Action<int> OnSkill;
        event Action OnLinkSkill;
        #endregion

        #region UI 事件
        event Action OnCloseView;
        event Action OnOpenOperatorDisplay;
        #endregion

        /// <summary>冻结/解冻角色输入（UI 打开时禁用 Player map）。</summary>
        void SetPlayerInputEnabled(bool enabled);
    }
}
