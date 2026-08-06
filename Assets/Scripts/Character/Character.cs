using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 全角色共享基类：移动/战斗状态机、驱动器、属性、战斗控制器的宿主。
    /// 干员(Operator)与敌人(Enemy)通过子类提供各自的 SO 数据。
    /// </summary>
    public abstract class Character : CharacterMovementControlBase
    {
        /// <summary>移动数据（子类从各自 SO 提供）</summary>
        public abstract CharacterMovementData MovementData { get; }
        /// <summary>战斗数据（子类从各自 SO 提供）</summary>
        public abstract CharacterCombatData CombatData { get; }
        /// <summary>属性数据（子类从各自 SO 提供，仅基类内部使用）</summary>
        protected abstract CharacterAttributeData AttributeData { get; }

        /// <summary>当前角色的移动状态机。</summary>
        public CharacterMovementStateMachine movementStateMachine { get; private set; }
        /// <summary>当前角色的战斗状态机。</summary>
        public CharacterCombatStateMachine combatStateMachine { get; private set; }
        /// <summary>移动驱动器：从外部(玩家输入或 AI）读取输入，向移动状态机写入数据。</summary>
        public CharacterMovementDriver movementDriver { get; private set; }
        /// <summary>战斗驱动器：从外部(玩家输入或 AI）读取输入，向战斗状态机写入数据。</summary>
        public CharacterCombatDriver combatDriver { get; private set; }
        /// <summary>战斗控制器：处理角色的战斗逻辑。</summary>
        public CharacterCombatController combatController { get; private set; }
        /// <summary>运行时属性，攻击伤害结算的唯一入口。</summary>
        public CharacterAttribute attribute { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            var attrComp = GetComponent<CharacterAttributeComponent>();
            attrComp.Init(AttributeData);
            attribute = attrComp.Attribute;
            combatController = new CharacterCombatController(_animator, transform, CombatData, attribute);
            movementDriver = new CharacterMovementDriver();
            combatDriver = new CharacterCombatDriver();
            movementStateMachine = new CharacterMovementStateMachine(this);
            combatStateMachine = new CharacterCombatStateMachine(this);
        }

        protected override void Start()
        {
            base.Start();
            movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
            combatStateMachine.ChangeState(CharacterCombatStateType.Null);
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
                    movementStateMachine.ChangeState(CharacterMovementStateType.Dash);
                    break;
                case OnEnterAnimationState.Idle:
                    movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
                    break;
                case OnEnterAnimationState.Sprint:
                    movementStateMachine.ChangeState(CharacterMovementStateType.Sprint);
                    break;
                case OnEnterAnimationState.ReturnRun:
                    movementStateMachine.ChangeState(CharacterMovementStateType.ReturnRun);
                    break;
                case OnEnterAnimationState.ATK:
                    combatStateMachine.ChangeState(CharacterCombatStateType.NormalATK);
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

        /// <summary>冲刺冷却结束回调，由 CharacterDashingState 调用。</summary>
        public void ResetDash()
        {
            movementDriver.canDash = true;
        }

        /// <summary>攻击冷却结束回调，由 CharacterCombatNullState 调用。</summary>
        public void CancelAttackColdTime()
        {
            combatController.CancelAttackColdTime();
        }

        /// <summary>绘制攻击检测盒（红色线框），Scene 视图常显，Game 视图需打开 Gizmos 按钮。</summary>
        private void OnDrawGizmos()
        {
            combatController?.DrawAttackGizmos();
        }
    }
}
