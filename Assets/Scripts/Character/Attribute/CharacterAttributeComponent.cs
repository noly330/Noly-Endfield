using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 角色属性组件：挂在任何拥有属性的角色上（干员、敌人、测试靶子）。
    /// 运行时创建 CharacterAttribute，供伤害结算读取：
    /// 攻击方从 Operator.attribute 拿，受击方从这里拿（GetComponentInParent）。
    /// </summary>
    public class CharacterAttributeComponent : MonoBehaviour
    {
        public CharacterAttribute Attribute { get; private set; }

        /// <summary>角色在 Awake 注入属性数据（干员用 operatorSO.attributeData，敌人以后同理）</summary>
        public void Init(CharacterAttributeData data)
        {
            Attribute = new CharacterAttribute(data);
        }
    }
}
