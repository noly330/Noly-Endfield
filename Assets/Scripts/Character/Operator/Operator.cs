using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员的状态机宿主。主控干员与队友干员共用此移动系统。
    /// </summary>
    public class Operator : CharacterMovementControlBase,IDamageable
    {
        /// <summary>/// 当前干员的移动状态机。/// </summary>
        public OperatorMovementStateMachine movementStateMachine { get; private set; }
        /// <summary>/// 当前干员的战斗状态机。/// </summary>
        public OperatorCombatStateMachine combatStateMachine { get; private set; }

        /// <summary>
        /// 当前干员的移动驱动器。从外部(玩家输入或者ai行为树）读取输入，向移动状态机写入数据。
        /// </summary>
        public OperatorMovementDriver movementDriver { get; private set; }
        /// <summary>
        /// 当前干员的战斗驱动器。从外部(玩家输入或者ai行为树）读取输入，向战斗状态机写入数据。
        /// </summary>
        public OperatorCombatDriver combatDriver { get; private set; }
        /// <summary>
        /// 当前干员的战斗控制器。负责处理干员的战斗逻辑。
        /// </summary>
        public OperatorCombatController combatController { get; private set; }

        public OperatorSO operatorSO;



        protected override void Awake()
        {
            base.Awake();
            combatController = new OperatorCombatController(_animator,this.transform,operatorSO.combatData);
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

        public void TakeDamage(DamageInfo damageInfo)
        {
            Debug.Log("我" + name + "被" + damageInfo.attacter.name + "打了，快来救我" );
        }
        #endregion
    }
}
