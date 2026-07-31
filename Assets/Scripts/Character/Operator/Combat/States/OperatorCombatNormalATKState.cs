using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            _combatStateMachine.ChangeState(_combatStateMachine.nullState);
        }
    }
}
