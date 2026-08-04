

using UnityEngine;


namespace Endfield
{
    /// <summary>
    /// 战斗空闲状态。不覆写任何逻辑，仅作为当前状态的占位，
    /// 让基类 OperatorCombatState.HandInput() 能被调用以检测攻击输入。
    /// </summary>
    public class OperatorCombatNullState : OperatorCombatState
    {
        public OperatorCombatNullState(OperatorCombatStateMachine stateMachine) : base(stateMachine)
        {
        }
        public override void Enter()
        {
            base.Enter();
            Debug.Log("进入战斗空状态");
            _combatStateMachine.combatController.CancelAttackColdTime();
        }

        public override void Update()
        {
            base.Update();
        }
        public override void OnAnimationTranslateEvent(IState state)
        {
            _combatStateMachine.ChangeState(state);
        }
    }
}
