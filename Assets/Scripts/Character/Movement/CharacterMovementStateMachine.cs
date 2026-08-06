using System.Collections.Generic;

namespace Endfield
{
    /// <summary>角色移动状态类型（字典索引）</summary>
    public enum CharacterMovementStateType
    {
        Idle,
        Walk,
        Run,
        Sprint,
        Dash,
        ReturnRun,
    }

    public class CharacterMovementStateMachine : StateMachine
    {
        public Character character { get; }
        public CharacterMovementReusableData reusableData { get; }

        private readonly Dictionary<CharacterMovementStateType, CharacterMovementState> _states = new();

        public CharacterMovementStateMachine(Character character)
        {
            this.character = character;
            reusableData = new CharacterMovementReusableData();

            _states[CharacterMovementStateType.Idle] = new CharacterIdlingState(this);
            _states[CharacterMovementStateType.Walk] = new CharacterWalkingState(this);
            _states[CharacterMovementStateType.Run] = new CharacterRunningState(this);
            _states[CharacterMovementStateType.Sprint] = new CharacterSprintingState(this);
            _states[CharacterMovementStateType.Dash] = new CharacterDashingState(this);
            _states[CharacterMovementStateType.ReturnRun] = new CharacterReturnRunState(this);
        }

        /// <summary>按类型取状态</summary>
        public CharacterMovementState GetState(CharacterMovementStateType type) => _states[type];

        /// <summary>按类型取状态（强类型）</summary>
        public T GetState<T>(CharacterMovementStateType type) where T : CharacterMovementState => _states[type] as T;

        /// <summary>按类型切换状态</summary>
        public void ChangeState(CharacterMovementStateType type) => ChangeState(_states[type]);
    }
}
