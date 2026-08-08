using System.Collections.Generic;

namespace Endfield
{
    public enum CharacterCombatStateType
    {
        Null,
        NormalATK,
        Skill,
        Link,
        Hit,
    }

    public class CharacterCombatStateMachine : StateMachine
    {
        public Character character { get; }
        public CharacterCombatReusableData resuableData { get; }
        public CharacterCombatController combatController { get; }

        private readonly Dictionary<CharacterCombatStateType, CharacterCombatState> _states = new();

        public CharacterCombatStateMachine(Character character)
        {
            resuableData = new CharacterCombatReusableData();
            this.character = character;
            combatController = character.combatController;

            _states[CharacterCombatStateType.Null] = new CharacterCombatNullState(this);
            _states[CharacterCombatStateType.NormalATK] = new CharacterCombatNormalATKState(this);
            _states[CharacterCombatStateType.Hit] = new CharacterCombatHitState(this);
        }

        /// <summary>按类型取状态</summary>
        public CharacterCombatState GetState(CharacterCombatStateType type) => _states[type];

        /// <summary>按类型取状态（强类型）</summary>
        public T GetState<T>(CharacterCombatStateType type) where T : CharacterCombatState => _states[type] as T;

        /// <summary>按类型切换状态</summary>
        public void ChangeState(CharacterCombatStateType type) => ChangeState(_states[type]);
    }
}
