using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 目标搜索策略抽象：AIController 依赖它（DIP），不依赖具体实现。
    /// 加新模式 = 新建子类 + CreateAssetMenu，不改现有代码（OCP）。
    /// </summary>
    public abstract class TargetSearchStrategySO : ScriptableObject
    {
        public abstract Transform FindTarget(Character owner, float radius);
    }
}
