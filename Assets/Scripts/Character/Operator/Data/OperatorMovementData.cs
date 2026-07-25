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
        [field: SerializeField] public OperatorDashData dashData { get; private set; }
        [field: SerializeField] public OperatorSprintData sprintData { get; private set; }
        [field: SerializeField] public OperatorReturnRunData returnRunData { get; private set; }
    }
}