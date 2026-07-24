using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    [System.Serializable]
    public class OperatorDashData
    {
        [field: SerializeField] public string frontDushAnimationName { get; private set; } = "Dash_Front";
        [field: SerializeField] public string backDushAnimationName { get; private set; } = "Dash_Back";
        [field: SerializeField] public float fadeTime { get; private set; } = 0.1555f;
        [field: SerializeField] public float rotationTime { get; private set; } = 0.09f;
        [field: SerializeField] public bool dodgeBackApplyRotation{ get; private set; } = false;    
        [field: SerializeField] public float coldTime { get; private set; } = 0.5f;
    }
}
