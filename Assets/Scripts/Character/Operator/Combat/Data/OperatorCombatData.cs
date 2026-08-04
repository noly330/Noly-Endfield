using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    [System.Serializable]
    public class OperatorCombatData : CharacterCombatData
    {

        [field: SerializeField] public CombatSetSO normalAttackData { get; private set; }
        [field: SerializeField] public CombatSetSO skillAttackData { get; private set; }
        [field: SerializeField] public CombatSetSO linkAttackData { get; private set; }
    }
}