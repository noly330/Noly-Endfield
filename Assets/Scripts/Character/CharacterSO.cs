using UnityEngine;
using Endfield.Module.Audio;

namespace Endfield
{
    /// <summary>
    /// 角色数据基类（ScriptableObject）：存放所有角色共有的数据（移动、AI、音效）。
    /// 干员/敌人通过子类补充各自的战斗与属性数据。
    /// </summary>
    public abstract class CharacterSO : ScriptableObject
    {
        [field: SerializeField] public CharacterMovementData movementData { get; private set; }
        [field: SerializeField] public CharacterAIData AIData { get; private set; }
        [field: SerializeField] public SoundData soundData { get; private set; }
    }
}
