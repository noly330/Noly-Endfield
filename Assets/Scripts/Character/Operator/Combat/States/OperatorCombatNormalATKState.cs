using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Endfield.Tools;

namespace Endfield
{
    /// <summary>
    /// 正常攻击状态。覆写 OperatorCombatState.HandInput() 检测攻击输入。
    /// </summary>
    public class OperatorCombatNormalATKState : OperatorCombatState
    {
        public OperatorCombatNormalATKState(OperatorCombatStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

        }
        public override void Update()
        {
            base.Update();
        }

        public override void OnAnimationTranslateEvent(IState state)
        {
            _combatStateMachine.ChangeState(state);
        }
        public override void OnAnimationExitEvent()
        {
            TimerManager.Instance.GetTimer(0.2f, OnBufferToNull);
        }

        private void OnBufferToNull()
        {
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsTag("ATK"))
            {
                _combatStateMachine.ChangeState(_combatStateMachine.nullState);
            }

        }
    }
}
