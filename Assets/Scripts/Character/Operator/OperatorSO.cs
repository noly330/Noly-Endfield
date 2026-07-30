using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    [CreateAssetMenu(menuName = "Endfield/Operator/OperatorSO")]
    public class OperatorSO : ScriptableObject
    {
        [field: SerializeField] public OperatorMovementData movementData{get; private set;}
    }

}
