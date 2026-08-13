using System;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;

namespace Endfield
{
    /// <summary>
    /// 全角色共享基类：移动/战斗状态机、驱动器、属性、战斗控制器的宿主。
    /// 干员(Operator)与敌人(Enemy)通过子类提供各自的 SO 数据。
    /// </summary>
    public abstract class Character : CharacterMovementControlBase, IBuffTarget
    {
        public abstract CharacterMovementData MovementData { get; }
        public abstract CharacterCombatData CombatData { get; }
        protected abstract CharacterAttributeData AttributeData { get; }
        public abstract CharacterAIData AIData { get; }
        /// <summary>技能攻击数据（干员有，敌人没有）。</summary>
        public virtual CombatSetSO SkillAttackData => null;
        public CharacterMovementStateMachine movementStateMachine { get; private set; }
        public CharacterCombatStateMachine combatStateMachine { get; private set; }
        public CharacterMovementDriver movementDriver { get; private set; }
        public CharacterCombatDriver combatDriver { get; private set; }
        public CharacterCombatController combatController { get; private set; }
        public CharacterAttribute attribute { get; private set; }
        /// <summary>buff 管理器（IBuffTarget 成员）。</summary>
        public BuffManager Buffs { get; private set; }
        /// <summary>受击结算器（订阅 OnDamaged 处理破防增/消）。</summary>
        public CharacterAbnormalityReceiver AbnormalityReceiver { get; private set; }
        /// <summary>受击方组件（供 DoT 扣血等读取）。</summary>

        // 显式实现 IBuffTarget.Attribute：复用现有 attribute，不改动原有代码的命名
        CharacterAttribute IBuffTarget.Attribute => attribute;
        public bool IsDead => _health != null && _health.isDead;
        /// <summary>死亡动画播完事件（供对象池回收等）。由 CharacterCombatDeadState 触发。</summary>
        public event Action OnDeathAnimationEnd;

        private CharacterHealth _health;
        public CharacterHealth Health => _health;

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

            _health = GetComponent<CharacterHealth>();
            if (_health != null)
            {
                _health.OnDamaged += OnHit;
                _health.OnDead += OnDead;
            }
            Buffs = new BuffManager(this);   // Character 实现了 IBuffTarget，buff 效果经窄接口访问
            AbnormalityReceiver = new CharacterAbnormalityReceiver(this);
        }


        protected void OnDestroy()
        {
            AbnormalityReceiver?.Dispose();
            if (_health != null)
            {
                _health.OnDamaged -= OnHit;
                _health.OnDead -= OnDead;
            }
        }

        protected override void Start()
        {
            base.Start();
            movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
            combatStateMachine.ChangeState(CharacterCombatStateType.Null);

            //测试:
            Buffs.Apply(BuffDB.Bleed,this);
        }

        protected override void Update()
        {
            base.Update();
            movementStateMachine.HandInput();
            movementStateMachine.Update();

            combatStateMachine.HandInput();
            combatStateMachine.Update();

            Buffs?.Update();   // buff 倒计时 + tick + 到期清理
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
                case OnEnterAnimationState.Skill:
                    combatStateMachine.ChangeState(CharacterCombatStateType.Skill);
                    movementStateMachine.ChangeState(CharacterMovementStateType.Null);   // 技能攻击移动
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
            // if (Time.time < _nextHitAnimTime) return;
            // _nextHitAnimTime = Time.time + 0.2f;

            if (string.IsNullOrEmpty(damageInfo.hitName)) return;
            if (!attribute.superArmor)
            {
                _animator.CrossFadeInFixedTime(damageInfo.hitName, 0.1f, 0);
                //朝向攻击者
                transform.LookAt(damageInfo.attacker.transform.position);
            }
        }
        /// <summary>
        /// 死亡回调
        /// </summary>
        private void OnDead()
        {
            combatStateMachine.ChangeState(CharacterCombatStateType.Dead);        // 战斗进终态
            movementStateMachine.ChangeState(CharacterMovementStateType.Null);    // 锁移动

            // 停止 AI（敌人）
            var aiCtrl = GetComponent<CharacterAIController>();
            if (aiCtrl != null) aiCtrl.enabled = false;
            var behaviorTree = GetComponent<BehaviorTree>();
            if (behaviorTree != null) behaviorTree.enabled = false;
            var navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent != null) navMeshAgent.enabled = false;
        }

        /// <summary>死亡动画播完通知（供对象池回收等）。由 CharacterCombatDeadState 调用。</summary>
        public void NotifyDeathAnimationEnd() => OnDeathAnimationEnd?.Invoke();

        /// <summary>绘制攻击检测盒（红色线框），Scene 视图常显，Game 视图需打开 Gizmos 按钮。</summary>
        private void OnDrawGizmos()
        {
            combatController?.DrawAttackGizmos();
        }
    }
}
