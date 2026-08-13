namespace Endfield
{
    /// <summary>
    /// 受击结算器（纯 C#）：订阅 OnDamaged，按攻击效果类型增/消破防。
    /// </summary>
    public class CharacterAbnormalityReceiver
    {
        private readonly Character _owner;
        private readonly CharacterHealth _health;

        public CharacterAbnormalityReceiver(Character owner)
        {
            _owner = owner;
            _health = owner.Health;
            if (_health != null) _health.OnDamaged += HandleAttackHit;
        }

        public void Dispose()
        {
            if (_health != null) _health.OnDamaged -= HandleAttackHit;
        }

        private void HandleAttackHit(DamageInfo info)
        {
            var caster = info.attacker != null ? info.attacker.GetComponent<Character>() : null;

            switch (info.attackEffectType)
            {
                case CombatAttackEffectType.Launch:
                    //TODO: 击飞/倒地动画（有破防时播起飞/倒地表现），动画到位后做
                    _owner.Buffs.Apply(BuffDB.DefenseBreak);   // Stack：+1层 + 刷新时长
                    break;

                case CombatAttackEffectType.KnockDown:
                    //TODO: 击飞/倒地动画（有破防时播起飞/倒地表现），动画到位后做
                    _owner.Buffs.Apply(BuffDB.DefenseBreak);   // Stack：+1层 + 刷新时长
                    break;

                case CombatAttackEffectType.Smash:
                    int smashConsumed = TryConsumeBreak();
                    if (smashConsumed > 0)
                    {
                        _owner.Buffs.Apply(BuffDB.SmashBurst, caster, smashConsumed);   // 猛击：高额爆发 ×1.25
                    }
                    else
                    {
                        _owner.Buffs.Apply(BuffDB.DefenseBreak);
                    }
                    break;

                case CombatAttackEffectType.Sunder:
                    int sunderConsumed = TryConsumeBreak();
                    if (sunderConsumed > 0)
                    {
                        _owner.Buffs.Apply(BuffDB.Vulnerable, caster, sunderConsumed);   // 脆弱
                        _owner.Buffs.Apply(BuffDB.SunderBurst, caster, sunderConsumed);  // 低额爆发 ×0.2
                    }
                    else
                    {
                        _owner.Buffs.Apply(BuffDB.DefenseBreak);
                    }
                    break;
            }
        }

        /// <summary>有破防则消耗全部层数（→ 广播 OnDefenseBreakConsumed{层数}）并返回层数；无破防返回 0。</summary>
        private int TryConsumeBreak()
        {
            var buffs = _owner.Buffs;
            if (!buffs.Has(BuffDB.DefenseBreak.buffId)) return 0;
            int consumed = buffs.GetStack(BuffDB.DefenseBreak.buffId);
            buffs.Remove(BuffDB.DefenseBreak.buffId);
            return consumed;
        }
    }
}
