using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    public static class Events
    {
        /// <summary>当破防buff添加的时候</summary>
        public struct OnDefenseBreakApplied{}

        /// <summary>当破防buff消耗的时候</summary>
        public struct OnDefenseBreakConsumed
        {
            public int breakStack;
        }

        /// <summary>当有干员打出连携技的时候（连携链）</summary>
        public struct OnLinkSkillTriggered { }
    }
}