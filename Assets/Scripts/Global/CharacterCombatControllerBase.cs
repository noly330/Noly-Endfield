using UnityEngine;
using Endfield.Tools;
namespace Endfield
{
    /// <summary>
    /// 用来处理通用的战斗逻辑
    /// </summary>
    public class CharacterCombatControllerBase
    {
        private Transform _characterTrans;
        private Animator _animator;
        private LayerMask _targetMask;
        private float _targetRadius;
        public bool canAttack { get; private set; }
        public CharacterCombatControllerBase(Animator animator, Transform characterTrans,
        CharacterCombatData characterCombatData)
        {
            _characterTrans = characterTrans;
            _animator = animator;
            _targetMask = characterCombatData.targetMask;
            _targetRadius = characterCombatData.targetRadius;
            canAttack = true;
        }

        public void SetAttackColdTime() => canAttack = false;

        public void CancelAttackColdTime() => canAttack = true;

        #region 攻击检测
        private CombatSetSO _currentCombatSet;
        private int _currentComboIndex;
        private int _firedDetectCount;

        public void StartAttackDetection(CombatSetSO combatSet, int comboIndex)
        {
            _currentCombatSet = combatSet;
            _currentComboIndex = comboIndex;
            _firedDetectCount = 0;
        }

        public void UpdateAttackDetection()
        {
            string attackName = _currentCombatSet.TryGetCombatName(_currentComboIndex);
            //TODO:以后动画分层的话，需要传递更多信息
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(attackName) ||
            _animator.IsInTransition(0))
            {
                return;
            }

            CombatDetectConfig detectConfig = _currentCombatSet.TryGetDetectConfig(_currentComboIndex, _firedDetectCount);
            CombatInteractionConfig interactionConfig = _currentCombatSet.TryGetInteractionConfig(_currentComboIndex, _firedDetectCount);
            if (detectConfig == null || interactionConfig == null)
            {
                return;
            }
            switch (detectConfig.detectType)
            {
                case CombatDetectType.None:
                    break;
                case CombatDetectType.Area:
                    UpdateAreaAttackDetection(detectConfig, interactionConfig);
                    break;
                case CombatDetectType.Single:
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 近战攻击检测逻辑
        /// </summary>
        private void UpdateAreaAttackDetection(CombatDetectConfig detectConfig, CombatInteractionConfig interactionConfig)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= detectConfig.startTime)
            {
                //盒子的检测中心
                Vector3 boxPosition = _characterTrans.transform.forward * detectConfig.position.z +
                                      _characterTrans.transform.up * detectConfig.position.y +
                                      _characterTrans.transform.right * detectConfig.position.x;
                Quaternion boxRotation = _characterTrans.transform.rotation * Quaternion.Euler(detectConfig.rotation);
                Collider[] targetList = Physics.OverlapBox(_characterTrans.transform.position + boxPosition,
                detectConfig.scale, boxRotation, _targetMask);

                //遍历敌人:
                foreach (Collider target in targetList)
                {
                    IDamageable damageable = target.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        //TODO: 伤害值需要从配置中获取,先用10f作为占位符
                        damageable.TakeDamage(new DamageInfo { attacter = _characterTrans.transform, damage = interactionConfig.damageMul });
                        Debug.Log("对敌人：" + target.name + "造成了" + interactionConfig.damageMul + "点伤害" + ",并且造成了受击动画：" + interactionConfig.hitName);
                    }
                }
                _firedDetectCount++;
            }
        }
        #endregion

        #region 索敌
        private Transform _cachedTarget;  //缓存目标
        private GameTimer _stickTimer;  //索敌定时器
        private float _stickTime = 2f; //索敌时间，TODO:先用硬编码测试

        public Transform GetCurrentTarget()
        {
            if (_cachedTarget)
            {
                return _cachedTarget;
            }
            return GetNearestEnemy();
        }

        /// <summary>
        /// 获取最近敌人
        /// </summary>
        /// <returns></returns>
        public Transform GetNearestEnemy()
        {
            Collider[] hits = Physics.OverlapSphere(_characterTrans.position, _targetRadius, _targetMask);

            Transform nearest = null;
            float minSqr = float.MaxValue;   // 用平方距离比较，省掉开平方开销

            foreach (Collider hit in hits)
            {
                if (!hit.TryGetComponent<IDamageable>(out _)) continue;

                float sqr = (hit.transform.position - _characterTrans.position).sqrMagnitude;
                if (sqr < minSqr)
                {
                    minSqr = sqr;
                    nearest = hit.transform;
                }
            }
            _cachedTarget = nearest;
            return nearest;
        }

        /// <summary>平滑转向目标</summary>
        public void FaceTarget(Transform target, float smoothTime = 60f)
        {
            Vector3 dir = target.position - _characterTrans.position;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.0001f) return;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            float t = 1 - Mathf.Exp(-smoothTime * Time.deltaTime);   // 帧无关平滑系数
            _characterTrans.rotation = Quaternion.Slerp(_characterTrans.rotation, lookRotation, t);
        }

        #endregion
    }
}
