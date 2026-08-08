using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 队伍配置：最多 4 个槽位，只放干员（OperatorSO）。槽位顺序 = 编队顺序（1~4）。
    /// 未来 UI 编队 = 读写这个 asset（换人 / 排序）。
    /// </summary>
    [CreateAssetMenu(menuName = "Endfield/Team/TeamSO")]
    public class TeamSO : ScriptableObject
    {
        public List<OperatorSO> slots = new List<OperatorSO>();
    }
}
