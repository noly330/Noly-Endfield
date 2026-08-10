using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 人形视野搜索：角色前方半角范围内的最近敌人（180°视野 = halfAngle 90°）。
    /// </summary>
    [CreateAssetMenu(menuName = "Endfield/AI/Search/FovCone")]
    public class FovConeSearchSO : TargetSearchStrategySO
    {
        //TODO: 视野半角写死，之后进 AI 配置
        [SerializeField] private float _halfAngle = 90f;

        public override Transform FindTarget(Character owner, float radius)
        {
            Transform origin = owner.transform;
            Vector3 forward = origin.forward;
            Collider[] hits = Physics.OverlapSphere(origin.position, 
            radius, owner.CombatData.targetMask);

            Transform nearest = null;
            float minSqr = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit.transform == origin || hit.transform.IsChildOf(origin)) continue;   // 排除自身
                if (!hit.TryGetComponent<IDamageable>(out var damageable)) continue;
                if (damageable.isDead) continue;   // 跳过死亡目标

                Vector3 dir = (hit.transform.position - origin.position).normalized;
                if (Vector3.Angle(forward, dir) > _halfAngle) continue;   // 视野外排除

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
