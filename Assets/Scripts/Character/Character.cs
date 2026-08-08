using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 全角色共享基类：移动/战斗状态机、驱动器、属性、战斗控制器的宿主。
    /// 干员(Operator)与敌人(Enemy)通过子类提供各自的 SO 数据。
    /// </summary>
    public abstract class Character : CharacterMovementControlBase
    {
        public abstract CharacterMovementData MovementData { get; }
        public abstract CharacterCombatData CombatData { get; }
        protected abstract CharacterAttributeData AttributeData { get; }
        public abstract CharacterAIData AIData { get; }
        public CharacterMovementStateMachine movementStateMachine { get; private set; }
        public CharacterCombatStateMachine combatStateMachine { get; private set; }
        public CharacterMovementDriver movementDriver { get; private set; }
        public CharacterCombatDriver combatDriver { get; private set; }
        public CharacterCombatController combatController { get; private set; }
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

            var health = GetComponent<CharacterHealth>();
            if (health != null) health.OnDamaged += OnHit;
        }

        protected void OnDestroy()
        {
            var health = GetComponent<CharacterHealth>();
            if (health != null) health.OnDamaged -= OnHit;
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
                    combatStateMachine.ChangeState(CharacterCombatStateType.Null);   // 闪避打断攻击
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
                    movementStateMachine.ChangeState(CharacterMovementStateType.Null);   // 攻击锁移动
                    break;
                case OnEnterAnimationState.Hit:
                    combatStateMachine.ChangeState(CharacterCombatStateType.Hit);
                    movementStateMachine.ChangeState(CharacterMovementStateType.Null);   // 受击锁移动
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

        /// <summary>
        /// 是否允许被切走（TeamManager 切人前查询）。
        /// 受击中不可切；未来技能/连携状态同样禁止。
        /// </summary>
        public bool CanSwitchOut()
        {
            return combatStateMachine.currentState.Value is not CharacterCombatHitState;
        }

        private float _nextHitAnimTime;   // 受击动画冷却时间戳

        /// <summary>
        /// 受击回调：只负责播受击动画，进入受击状态由受击动画的
        /// OnAnimationTranslate(Hit) 路由完成（与 ATK 机制一致）。
        /// </summary>
        private void OnHit(DamageInfo damageInfo)
        {
            if (Time.time < _nextHitAnimTime) return;
            _nextHitAnimTime = Time.time + 0.2f;

            if (string.IsNullOrEmpty(damageInfo.hitName)) return;
            _animator.CrossFadeInFixedTime(damageInfo.hitName, 0.1f, 0);
            //朝向攻击者
            transform.LookAt(damageInfo.attacker.transform.position);
        }

        /// <summary>绘制攻击检测盒（红色线框），Scene 视图常显，Game 视图需打开 Gizmos 按钮。</summary>
        private void OnDrawGizmos()
        {
            combatController?.DrawAttackGizmos();
        }
    }
}
