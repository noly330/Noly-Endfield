using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Global
{
    public class CharacterMovementControlBase : MonoBehaviour
    {
        protected CharacterController _characterController;
        protected Animator _animator;
        public Animator animator => _animator;
        protected Vector3 _moveDirection;

        //地面检测
        [Header("地面检测")]
        [SerializeField] public bool isGround;
        [SerializeField] public bool isFall;
        [SerializeField] protected float _groundDetectionPositionOffset;
        [SerializeField] protected float _deyectionRange;
        [SerializeField] protected LayerMask _whatIsGround;

        //重力相关
        protected readonly float _gravity = -9.8f;
        [SerializeField] protected float _verticalVelocity;  //垂直速度
        protected float _verticalMaxVelocity = -18f;  //低于这个值的时候才应用重力
        protected float _fallOutDeltaTime;
        protected float _fallOutTime = 0.15f;  //防止角色下楼梯鬼畜
        protected Vector3 _verticalDirection;  //Y轴移动方向

        protected virtual void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {
            SetCharacterGravity();
            SetCharacterFall();
            //UpdateCharacterGravity();
        }

        protected virtual void OnAnimatorMove()
        {
            _animator.ApplyBuiltinRootMotion();
            UpdateCharacterMoveDirection(_animator.deltaPosition);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        }


        private bool GroundDetection()
        {
            var detectionPosition = new Vector3(transform.position.x, transform.position.y - _groundDetectionPositionOffset, transform.position.z);
            return Physics.CheckSphere(detectionPosition, _deyectionRange, _whatIsGround, QueryTriggerInteraction.Ignore);  // 忽略触发器
        }

        private void SetCharacterGravity()
        {
            isGround = GroundDetection();

            if (isGround)  //在地面
            {
                _fallOutDeltaTime = _fallOutTime;

                if (_verticalVelocity < 2f)
                    _verticalVelocity = -2f;
            }
            else
            {
                if (_fallOutDeltaTime > 0f)
                    _fallOutDeltaTime -= Time.deltaTime;  //等待0.15秒，来帮助角色从较低高度差下落
                else
                {
                    //说明角色还没落地

                    if (_verticalVelocity > _verticalMaxVelocity)
                    {
                        _verticalVelocity += _gravity * 1.5f * Time.deltaTime;
                    }
                }
            }
        }
        private void SetCharacterFall()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Hurt") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") ||
            isGround)
            {
                isFall = false;
                return;
            }
            if (_verticalVelocity < -2.2f)
                isFall = true;
        }

        //坡道检测
        private Vector3 SlopResetDirection(Vector3 moveDirection)
        {
            if (Physics.Raycast(transform.position + (transform.up * 5f), Vector3.down, out RaycastHit hit, _characterController.height * 0.85f
            , _whatIsGround, QueryTriggerInteraction.Ignore))
            {
                if (Vector3.Dot(Vector3.up, hit.normal) != 0f)  //点积等于0说明两个向量垂直
                {
                    return moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                }
            }
            return moveDirection;
        }

        protected void UpdateCharacterMoveDirection(Vector3 direction)
        {
            _moveDirection = SlopResetDirection(direction);
            _verticalDirection.Set(0f, _verticalVelocity, 0f);

            _characterController.Move((_moveDirection + _verticalDirection) * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            var detectionPosition = new Vector3(transform.position.x, transform.position.y - _groundDetectionPositionOffset, transform.position.z);
            Gizmos.DrawWireSphere(detectionPosition, _deyectionRange);
        }

    }
}