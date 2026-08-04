using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    public class OperatorCombatController : CharacterCombatControllerBase
    {
        public OperatorCombatController(Animator animator,Transform characterTrans,
        OperatorCombatData operatorCombatData) : base(animator,characterTrans,operatorCombatData)
        {
        }
    }
}