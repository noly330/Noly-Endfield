using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    [System.Serializable]
    public class OperatorMovementData
    {
        [field: SerializeField] public OperatorRunData runData { get; private set; }
        [field: SerializeField] public OperatorWalkData walkData { get; private set; }

    }
}