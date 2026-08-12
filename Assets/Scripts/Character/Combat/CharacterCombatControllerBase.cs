using UnityEngine;
using Endfield.Module.Timer;
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
        private CharacterAttribute _attackerAttribute;
        public bool canAttack { get; private set; }
        public CharacterCombatControllerBase(Animator animator, Transform characterTrans,
        CharacterCombatData characterCombatData, CharacterAttribute attackerAttribute)
        {
            _characterTrans = characterTrans;
            _animator = animator;
            _targetMask = characterCombatData.targetMask;
            _targetRadius = characterCombatData.targetRadius;
            _attackerAttribute = attackerAttribute;
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
                    UpdateSingleAttackDetection(detectConfig, interactionConfig);
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
                        // 攻击方只算出伤，防御减免由受击方 TakeDamage 自行处理
                        float rawDamage = DamageCalculator.CalculateRawDamage(_attackerAttribute, interactionConfig.damageMul);
                        damageable.TakeDamage(new DamageInfo { attacker = _characterTrans, rawDamage = rawDamage, hitName = interactionConfig.hitName });
                        //Debug.Log("对敌人：" + target.name + "出伤" + rawDamage + "（未减防御）");
                    }
                    if (target.transform == _cachedTarget)
                    {
                        SetCachedTarget(target.transform);   // 打中缓存目标 → 重新计时，不切换到其他敌人
                    }
                }
                _firedDetectCount++;
            }
        }

        /// <summary>
        /// 远程指定目标的攻击检测逻辑
        /// </summary>
        private void UpdateSingleAttackDetection(CombatDetectConfig detectConfig, CombatInteractionConfig interactionConfig)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= detectConfig.startTime)
            {
                Transform target = GetCurrentTarget();
                if (target == null || !target.TryGetComponent<IDamageable>(out var damageable))
                    return;
                float rawDamage = DamageCalculator.CalculateRawDamage(_attackerAttribute, interactionConfig.damageMul);
                damageable.TakeDamage(new DamageInfo { attacker = _characterTrans, rawDamage = rawDamage, hitName = interactionConfig.hitName });
                //Debug.Log("对敌人：" + target.name + "出伤" + rawDamage + "（未减防御）");

                SetCachedTarget(target);   // 命中刷新粘性缓存
                _firedDetectCount++;

            }
        }

        #endregion

        #region 索敌
        private Transform _cachedTarget;  //缓存目标
        private GameTimer _stickTimer;  //粘性倒计时定时器
        private float _stickTime = 2f; //索敌时间，TODO:先用硬编码测试

        /// <summary>锁目标并重启 2 秒粘性倒计时（打中缓存目标时也走这个刷新）</summary>
        private void SetCachedTarget(Transform target)
        {
            _cachedTarget = target;
            if (_stickTimer != null) TimerManager.Instance.UnregisterTimer(_stickTimer);
            _stickTimer = TimerManager.Instance.GetTimer(_stickTime, OnStickExpired);
        }

        /// <summary> x秒没打中缓存目标 → 清缓存，下次 GetCurrentTarget 回退最近索敌</summary>
        private void OnStickExpired()
        {
            _cachedTarget = null;
        }

        public Transform GetCurrentTarget()
        {
            if (_cachedTarget)
            {
                // 缓存目标死亡或超出索敌半径 → 重新最近索敌（GetNearestEnemy 会重新锁定）
                if (!_cachedTarget.TryGetComponent<IDamageable>(out var dmg) || dmg.isDead)
                {
                    return GetNearestEnemy();
                }
                Vector3 toTarget = _cachedTarget.position - _characterTrans.position;
                if (toTarget.sqrMagnitude > _targetRadius * _targetRadius)
                {
                    return GetNearestEnemy();
                }
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
                if (!hit.TryGetComponent<IDamageable>(out var damageable)) continue;
                if (damageable.isDead) continue;   // 跳过死亡目标

                float sqr = (hit.transform.position - _characterTrans.position).sqrMagnitude;
                if (sqr < minSqr)
                {
                    minSqr = sqr;
                    nearest = hit.transform;
                }
            }
            if (nearest != null)
            {
                SetCachedTarget(nearest);   // 初始锁定最近敌人并开启粘性倒计时
            }
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
        /// <summary>
        /// 在 Scene/Game 视图绘制当前攻击检测盒，方便调试命中范围。
        /// 由 Operator.OnDrawGizmos 调用，仅在普攻动画期间绘制。
        /// </summary>
        public void DrawAttackGizmos()
        {
            if (_currentCombatSet == null) return;
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsTag("ATK")&&
            !_animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) return;

            // 当前正在等待触发的检测窗口
            CombatDetectConfig detectConfig = _currentCombatSet.TryGetDetectConfig(_currentComboIndex, _firedDetectCount);
            if (detectConfig == null) return;

            // 与 UpdateAreaAttackDetection 完全相同的盒体计算
            Vector3 boxPosition = _characterTrans.forward * detectConfig.position.z +
                                  _characterTrans.up * detectConfig.position.y +
                                  _characterTrans.right * detectConfig.position.x;
            Quaternion boxRotation = _characterTrans.rotation * Quaternion.Euler(detectConfig.rotation);
            Vector3 boxCenter = _characterTrans.position + boxPosition;

            // OverlapBox 用 detectConfig.scale 作半尺寸，实际盒体全尺寸 = scale * 2
            Vector3 boxSize = detectConfig.scale * 2f;

            Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxRotation, Vector3.one);

            // 半透明红色填充：Game 视图比细线醒目得多
            Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
            Gizmos.DrawCube(Vector3.zero, boxSize);

            // 红色线框描边
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
