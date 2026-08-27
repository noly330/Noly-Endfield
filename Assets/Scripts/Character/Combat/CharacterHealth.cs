using System;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 受击方组件：纯数值层，负责当前血量资源与伤害接收。
    /// 攻击方只提供原始伤害，防御减免在 TakeDamage 内用自身属性自行计算；
    /// 受击表现（动画/顿帧/特效）由 OnDamaged 事件的订阅者处理，本组件不碰表现。
    /// </summary>
    public class CharacterHealth : MonoBehaviour, IDamageable
    {
        private CharacterAttributeComponent _attributeComponent;
        private Character _character;
        private float _currentHP;
        private bool _isDead;
        public event Action<DamageInfo> OnDamaged;
        public event Action OnDead;
        public float CurrentHP => _currentHP;
        public float MaxHP => _attributeComponent?.Attribute?.MaxHP ?? 0f;
        public bool isDead => _isDead;

        private void Awake()
        {
            _attributeComponent = GetComponent<CharacterAttributeComponent>();
            _character = GetComponent<Character>();
        }

        private void Start()
        {
            _currentHP = MaxHP;
        }

        private void Update()
        {
            
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_isDead) return;

            // 角色状态结算（仅直接攻击受闪避影响；DoT/Status 不受）：
            // 完美闪避 → 无视正常攻击；闪避 → 触发完美闪避（一次冲刺一次由状态切换保证）
            if (damageInfo.damageType == DamageType.Direct && _character != null)
            {
                if (_character.State == CharacterState.PerfectDodging) return;
                if (_character.State == CharacterState.Dodging)
                {
                    _character.TriggerPerfectDodge();
                    return;
                }
            }

            // 受击方自行减免防御（属性未初始化时按防御 0），再乘受伤加深（碎甲等）
            var attr = _attributeComponent?.Attribute;
            float def = attr?.Def ?? 0f;
            float finalDamage = DamageCalculator.ApplyDefense(damageInfo.rawDamage, def) * (1f + (attr?.DamageTakenPercent ?? 0f));
            //Debug.Log($"受到伤害：{finalDamage}");
            _currentHP = Mathf.Max(_currentHP - finalDamage, 0f);

            // 受击事件（表现层订阅，本组件不做表现）
            OnDamaged?.Invoke(damageInfo);

            if (_currentHP <= 0f && !_isDead)
            {
                _isDead = true;
                OnDead?.Invoke();   // 死亡处理下一步接
            }
        }

        /// <summary>回血</summary>
        public void Heal(float amount)
        {
            if (_isDead) return;
            _currentHP = Mathf.Min(_currentHP + amount, MaxHP);
        }

        /// <summary>对象池复用重置：回满血、清死旗。</summary>
        public void Reset()
        {
            _currentHP = MaxHP;
            _isDead = false;
        }
    }
}
