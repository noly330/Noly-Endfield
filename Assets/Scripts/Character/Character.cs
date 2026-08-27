using System;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using Endfield.Module.Audio;

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
        public abstract SoundData SoundData { get; }  //角色音效数据
        public void PlayVoice(SoundStyle style, int index = -1)  //让状态机方便调用
            => AudioService.Instance.Play(style, SoundData, transform.position, index);
        public virtual CombatSetSO SkillAttackData => null;  //技能数据，子类重写
        public virtual CombatSetSO LinkAttackData => null;  //连携技数据
        public virtual float LinkCooldown => 0f;  //连携技冷却
        public CharacterMovementStateMachine movementStateMachine { get; private set; }
        public CharacterCombatStateMachine combatStateMachine { get; private set; }
        public CharacterMovementDriver movementDriver { get; private set; }
        public CharacterCombatDriver combatDriver { get; private set; }
        public CharacterCombatController combatController { get; private set; }
        public CharacterAttribute attribute { get; private set; }
        public CharacterState State { get; private set; } = CharacterState.Normal;
        public BuffManager Buffs { get; private set; }  //Buff管理器
        public CharacterAbnormalityReceiver AbnormalityReceiver { get; private set; } //受击结算器(buff用)TODO:设计可能有问题，以后改
        CharacterAttribute IBuffTarget.Attribute => attribute;
        public bool IsDead => _health != null && _health.isDead;
        public event Action OnDeathAnimationEnd;  //死亡动画播完事件
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

        /// <summary>切换角色状态（由移动/战斗状态机调用）。</summary>
        public void SetState(CharacterState state) => State = state;

        /// <summary>闪避中被直接攻击 → 进入完美闪避状态（Enter 触发全局减速）。一次冲刺一次由状态切换保证。</summary>
        public void TriggerPerfectDodge()
        {
            movementStateMachine.ChangeState(CharacterMovementStateType.PerfectDodge);
        }

        private PerfectDodgeVisual _perfectDodgeVisual;

        /// <summary>触发完美闪避表现（蓝白轮廓 + 残影）。转给 PerfectDodgeVisual 组件，本类不直接碰表现。</summary>
        public void PlayPerfectDodgeVisual(float duration)
        {
            if (_perfectDodgeVisual == null)
                _perfectDodgeVisual = GetComponent<PerfectDodgeVisual>() ?? gameObject.AddComponent<PerfectDodgeVisual>();
            _perfectDodgeVisual?.Play(duration);
        }

        /// <summary>攻击冷却结束回调，由 CharacterCombatNullState 调用。</summary>
        public void CancelAttackColdTime()
        {
            combatController.CancelAttackColdTime();
        }

        /// <summary>
        /// 瞬移并同步内部状态（CharacterController / NavMeshAgent）。
        /// 放本类：谁瞬移谁负责自己的组件同步，调用方不用摸内脏（LoD）。
        /// </summary>
        public void TeleportTo(Vector3 pos, Quaternion rot)
        {
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            if (cc != null) cc.enabled = true;

            var nav = GetComponent<NavMeshAgent>();
            if (nav != null)
            {
                nav.enabled = true;
                nav.Warp(pos);
                nav.ResetPath();
            }
        }

        /// <summary>
        /// 是否允许切人。
        /// </summary>
        public bool CanSwitchOut()
        {
            return combatStateMachine.currentState.Value is not CharacterCombatHitState;
        }

        /// <summary>连携 CD：下次可入队的时间戳。</summary>
        public float NextLinkTime { get; private set; }

        /// <summary>连携 CD 是否已好（可入队/可打出）。</summary>
        public bool LinkReady => Time.time >= NextLinkTime;

        /// <summary>连携实际打出时重置 CD</summary>
        public void ResetLinkCooldown() => NextLinkTime = Time.time + LinkCooldown;

        /// <summary>是否允许打出连携</summary>
        public bool CanCastLink()
        {
            return combatStateMachine.currentState.Value is not CharacterCombatSkillState
                and not CharacterCombatHitState
                and not CharacterCombatDeadState;
        }

        private float _nextHitAnimTime;   // 受击动画冷却时间戳

        /// <summary>
        /// 受击回调：只负责播受击动画，进入受击状态由受击动画的
        /// </summary>
        private void OnHit(DamageInfo damageInfo)
        {
            if (Time.time < _nextHitAnimTime) return;
            _nextHitAnimTime = Time.time + 0.05f;

            if (string.IsNullOrEmpty(damageInfo.hitName)) return;
            if (State != CharacterState.SuperArmor)   // 霸体：受伤但不播受击动画
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

        /// <summary>死亡动画播完通知。由 CharacterCombatDeadState 调用。</summary>
        public void NotifyDeathAnimationEnd() => OnDeathAnimationEnd?.Invoke();  //目前就辅助回收对象池

        /// <summary>绘制攻击检测盒。</summary>
        private void OnDrawGizmos()
        {
            combatController?.DrawAttackGizmos();
        }
    }
}
