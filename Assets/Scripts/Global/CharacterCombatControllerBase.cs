using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Endfield
{
    /// <summary>
    /// 用来处理通用的战斗逻辑
    /// </summary>
    public  class CharacterCombatControllerBase
    {
        private Animator _animator;
        public bool canAttack{get;private set;}
        public CharacterCombatControllerBase(Animator animator)
        {
            _animator = animator;
            canAttack = true;
        }

        public void SetAttackColdTime() =>canAttack = false;
       
        public void CancelAttackColdTime() =>canAttack = true;
        
    }
}
