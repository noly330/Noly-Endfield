using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 通用 AI 配置数据（可序列化数据类，内嵌在角色 SO 中）。
    /// 干员与敌人共用；由 CharacterAIController 与行为树节点读取。
    /// </summary>
    [System.Serializable]
    public class CharacterAIData
    {
        public float searchRadius = 10f;        // AI搜索目标的半径
        public float searchInterval = 0.2f;     // 搜索间隔
        public float attackRange = 2.5f;        // 攻击范围
        public float stopDistance = 2.5f;       // 追击停距
    }
}
