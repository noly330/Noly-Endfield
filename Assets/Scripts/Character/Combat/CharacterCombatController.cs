using UnityEngine;

namespace Endfield
{
    public class CharacterCombatController : CharacterCombatControllerBase
    {
        public CharacterCombatController(Animator animator, Transform characterTrans,
        CharacterCombatData characterCombatData, CharacterAttribute attribute) : base(animator, characterTrans, characterCombatData, attribute)
        {
        }
    }
}
