using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 360° 全方位搜索：范围内最近的敌人。
    /// </summary>
    [CreateAssetMenu(menuName = "Endfield/AI/Search/OmniDirectional")]
    public class OmniDirectionalSearchSO : TargetSearchStrategySO
    {
        public override Transform FindTarget(Character owner, float radius)
        {
            Transform origin = owner.transform;
            Collider[] hits = Physics.OverlapSphere(origin.position, radius, owner.CombatData.targetMask);

            Transform nearest = null;
            float minSqr = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit.transform == origin || hit.transform.IsChildOf(origin)) continue;   // 排除自身
                if (!hit.TryGetComponent<IDamageable>(out _)) continue;

                float sqr = (hit.transform.position - origin.position).sqrMagnitude;
                if (sqr < minSqr)
                {
                    minSqr = sqr;
                    nearest = hit.transform;
                }
            }
            return nearest;
        }
    }
}
