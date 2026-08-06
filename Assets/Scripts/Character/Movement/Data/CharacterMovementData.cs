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
    }
}
