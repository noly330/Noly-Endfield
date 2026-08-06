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
        private float _currentHP;
        private bool _isDead;

        /// <summary>受击事件：携带伤害信息，供受击反馈 / 血条 UI 订阅。</summary>
        public event Action<DamageInfo> OnDamaged;
        /// <summary>死亡事件（死亡的具体处理下一步做，先留接口）。</summary>
        public event Action OnDead;

        /// <summary>当前血量。</summary>
        public float CurrentHP => _currentHP;
        /// <summary>血量上限，实时读属性，不缓存；属性未初始化时按 0。</summary>
        public float MaxHP => _attributeComponent?.Attribute?.MaxHP ?? 0f;

        private void Awake()
        {
            _attributeComponent = GetComponent<CharacterAttributeComponent>();
        }

        private void Start()
        {
            // Start 初始化：保证 CharacterAttributeComponent.Init 已执行（Awake 顺序不定）
            _currentHP = MaxHP;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_isDead) return;

            // 受击方自行减免防御（属性未初始化时按防御 0）
            float def = _attributeComponent?.Attribute?.Def ?? 0f;
            float finalDamage = DamageCalculator.ApplyDefense(damageInfo.rawDamage, def);
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

        /// <summary>回血，不超上限。</summary>
        public void Heal(float amount)
        {
            if (_isDead) return;
            _currentHP = Mathf.Min(_currentHP + amount, MaxHP);
        }
    }
}
