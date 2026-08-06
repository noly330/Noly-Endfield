using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    [System.Serializable]
    public class CharacterReturnRunData
    {
        [field: SerializeField] public float speedMult { get; private set; } = 1f;
        [field: SerializeField] public float inputMult { get; private set; } = 2.5f;
        [field: SerializeField] public float rotationTime { get; private set; } = 0.5f;
    }
}
