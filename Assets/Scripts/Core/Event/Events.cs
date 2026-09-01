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

        /// <summary>连携技打出 → 左侧弹干员连携照片（LinkBannerWidget 订阅）</summary>
        public struct LinkSkillCast
        {
            public Sprite linkHead;
        }

        /// <summary>连携队列变化（入队/出队）→ 队列头像 Widget 刷新</summary>
        public struct LinkSkillQueueChanged { }

        /// <summary>角色受到最终伤害（防御减免后）→ 伤害飘字</summary>
        public struct OnCharacterDamaged
        {
            public float damage;     // 最终伤害（已减防/加深）
            public bool isCrit;      // 是否暴击
            public Vector3 hitPos;   // 受击世界坐标
        }
    }
}