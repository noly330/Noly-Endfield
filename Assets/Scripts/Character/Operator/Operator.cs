using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员的状态机宿主。主控干员与队友干员共用此系统
    /// </summary>
    public class Operator : CharacterMovementControlBase
    {
        public OperatorMovementStateMachine movementStateMachine { get; private set; }
        public OperatorCombatStateMachine combatStateMachine { get; private set; }
        public OperatorMovementDriver movementDriver { get; private set; }
        public OperatorCombatDriver combatDriver { get; private set; }
        public OperatorCombatController combatController { get; private set; }
        public CharacterAttribute attribute { get; private set; }
        public OperatorSO operatorSO;

        protected override void Awake()
        {
            base.Awake();
            var attrComp = GetComponent<CharacterAttributeComponent>();
            attrComp.Init(operatorSO.attributeData);
            attribute = attrComp.Attribute;
            combatController = new OperatorCombatController(_animator,this.transform,operatorSO.combatData, attribute);
            movementDriver = new OperatorMovementDriver();
            combatDriver = new OperatorCombatDriver();
            movementStateMachine = new OperatorMovementStateMachine(this);
            combatStateMachine = new OperatorCombatStateMachine(this);
        }

        protected override void Start()
        {
            base.Start();
            movementStateMachine.ChangeState(movementStateMachine.idlingState);
            combatStateMachine.ChangeState(combatStateMachine.nullState);
        }

        protected override void Update()
        {
            base.Update();
            movementStateMachine.HandInput();
            movementStateMachine.Update();

            combatStateMachine.HandInput();
            combatStateMachine.Update();
        }

        public void OnAnimationTranslate(OnEnterAnimationState state)
        {
            switch (state)
            {
                case OnEnterAnimationState.Dash:
                    movementStateMachine.ChangeState(movementStateMachine.dashingState);
                    break;
                case OnEnterAnimationState.Idle:
                    movementStateMachine.ChangeState(movementStateMachine.idlingState);
                    break;
                case OnEnterAnimationState.Sprint:
                    movementStateMachine.ChangeState(movementStateMachine.sprintingState);
                    break;
                case OnEnterAnimationState.ReturnRun:
                    movementStateMachine.ChangeState(movementStateMachine.returnRunState);
                    break;
                case OnEnterAnimationState.ATK:
                    combatStateMachine.ChangeState(combatStateMachine.normalATKState);
                    break;
            }
        }
        public void OnAnimationEixt()
        {
            movementStateMachine.OnAnimationExitEvent();
            combatStateMachine.OnAnimationExitEvent();
        }
        public virtual Vector3 GetMovementInput()
        {
            return movementDriver.worldDirection;
        }

        public bool GetShouldWalk()
        {
            return movementDriver.shouldWalk;
        }

        /// <summary>冲刺冷却结束回调，由 OperatorDashingState 调用。</summary>
        public void ResetDash()
        {
            movementDriver.canDash = true;
        }

        #region 动画帧事件回调
        /// <summary>攻击冷却结束回调，由 OperatorCombatNullState 调用。</summary>
        public void CancelAttackColdTime()
        {
            combatController.CancelAttackColdTime();
        }
        #endregion

        /// <summary>绘制攻击检测盒（红色线框），Scene 视图常显，Game 视图需打开 Gizmos 按钮。</summary>
        private void OnDrawGizmos()
        {
            combatController?.DrawAttackGizmos();
        }
    }
}
