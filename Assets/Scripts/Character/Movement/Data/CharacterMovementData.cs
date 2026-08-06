using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    [System.Serializable]
    public class CharacterMovementData
    {
        [field: SerializeField] public CharacterRunData runData { get; private set; }
        [field: SerializeField] public CharacterWalkData walkData { get; private set; }
        [field: SerializeField] public CharacterDashData dashData { get; private set; }
        [field: SerializeField] public CharacterSprintData sprintData { get; private set; }
        [field: SerializeField] public CharacterReturnRunData returnRunData { get; private set; }

        // 攻击期间移动空状态配置（参考 ZZZ demo）
        [field: SerializeField] public float comboRotationTime { get; private set; } = 0.09f;       // 攻击时转向平滑时间
        [field: SerializeField] public float comboRotationPercentage { get; private set; } = 0.3f;  // 攻击前段可转向窗口(0~1)
    }
}
