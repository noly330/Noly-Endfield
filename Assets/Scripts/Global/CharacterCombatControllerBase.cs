using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Endfield
{
    /// <summary>
    /// 用来处理通用的战斗逻辑
    /// </summary>
    public class CharacterCombatControllerBase
    {
        private Transform _characterTrans;
        private Animator _animator;
        public bool canAttack { get; private set; }
        public CharacterCombatControllerBase(Animator animator,Transform characterTrans)
        {
            _characterTrans = characterTrans;
            _animator = animator;
            canAttack = true;
        }

        public void SetAttackColdTime() => canAttack = false;

        public void CancelAttackColdTime() => canAttack = true;

        #region 攻击检测
        private CombatSetSO _currentCombatSet;
        private int _currentComboIndex;
        private int _firedDetectCount;

        public void StartAttackDetection(CombatSetSO combatSet,int comboIndex)
        {
            _currentCombatSet = combatSet;
            _currentComboIndex = comboIndex;
            _firedDetectCount = 0;
        }

        public void UpdateAttackDetection()
        {
            string attackName = _currentCombatSet.TryGetCombatName(_currentComboIndex);
            //TODO:以后动画分层的话，需要传递更多信息
            if(!_animator.GetCurrentAnimatorStateInfo(0).IsName(attackName) ||
            _animator.IsInTransition(0))
            {
                return;
            }

            CombatDetectConfig detectConfig = _currentCombatSet.TryGetDetectConfig(_currentComboIndex,_firedDetectCount);
            CombatInteractionConfig interactionConfig = _currentCombatSet.TryGetInteractionConfig(_currentComboIndex,_firedDetectCount);
            if(detectConfig == null || interactionConfig == null)
            {
                return;
            }
            switch (detectConfig.detectType)
            {
                case CombatDetectType.None:
                    break;
                case CombatDetectType.Area:
                    UpdateAreaAttackDetection(detectConfig,interactionConfig);
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
        private void UpdateAreaAttackDetection(CombatDetectConfig detectConfig,CombatInteractionConfig interactionConfig)
        {
            if(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= detectConfig.startTime)
            {
                //盒子的检测中心
                Vector3 boxPosition = _characterTrans.transform.forward * detectConfig.position.z +
                                      _characterTrans.transform.up * detectConfig.position.y +
                                      _characterTrans.transform.right * detectConfig.position.x;
                Quaternion boxRotation = _characterTrans.transform.rotation * Quaternion.Euler(detectConfig.rotation);
                Collider[] targetList = Physics.OverlapBox(_characterTrans.transform.position + boxPosition,
                detectConfig.scale, boxRotation,detectConfig.targetMask);

                //遍历敌人:
                foreach (Collider target in targetList)
                {
                    IDamageable damageable = target.GetComponent<IDamageable>();
                    if(damageable != null)
                    {
                        //TODO: 伤害值需要从配置中获取,先用10f作为占位符
                        damageable.TakeDamage(new DamageInfo { attacter = _characterTrans.transform,damage = interactionConfig.damageMul });
                        Debug.Log("对敌人：" + target.name + "造成了" + interactionConfig.damageMul + "点伤害" + ",并且造成了受击动画：" + interactionConfig.hitName);
                    }
                }
                _firedDetectCount++;
            }
        }
        #endregion
    }


}
