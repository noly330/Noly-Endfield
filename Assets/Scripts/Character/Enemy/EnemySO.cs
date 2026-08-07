using UnityEngine;

namespace Endfield
{

    /// </summary>
    [CreateAssetMenu(menuName = "Endfield/Enemy/EnemySO")]
    public class EnemySO : CharacterSO
    {
        //TODO:先用角色通用，如果敌人有特殊逻辑以后写敌人专属的
        [field: SerializeField] public CharacterCombatData combatData { get; private set; }
        [field: SerializeField] public CharacterAttributeData attributeData { get; private set; }
    }
}
