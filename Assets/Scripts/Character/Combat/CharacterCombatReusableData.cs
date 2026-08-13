using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    public class CharacterCombatReusableData
    {
        public int combatIndex { get; set; } = 0;
        public int nextCombatIndex { get; set; }
        /// <summary>本次技能/连携的 CombatSetSO（由触发处注入，SkillState 数据驱动读取）。</summary>
        public CombatSetSO currentSkillData { get; set; }
    }
}
