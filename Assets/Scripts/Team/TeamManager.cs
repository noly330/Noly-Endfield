using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;
using BehaviorDesigner.Runtime;

namespace Endfield
{
    /// <summary>
    /// 队伍管理器：负责队伍组合、当前主控干员、切人（交换位置 + 控制权 + 相机）。
    /// 只处理干员（Operator）；未来 UI 编队通过 TeamSO 换人/排序。
    /// </summary>
    /// TODO:队伍管理器目前有问题，后期等addressable的资源加载写好以后，重构一下
    public class TeamManager : MonoBehaviour
    {
        private static TeamManager _instance;
        public static TeamManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<TeamManager>();
                return _instance;
            }
        }

        [Tooltip("队伍配置（槽位 1~4，只放干员）")]
        public TeamSO team;

        private readonly Dictionary<OperatorSO, Operator> _operators = new Dictionary<OperatorSO, Operator>();
        private int _activeSlot;
        private CinemachineVirtualCamera _virtualCamera;
        private ThirdPersonCamera _thirdPersonCamera;

        /// <summary>当前玩家控制的干员。</summary>
        public Operator ActiveOperator { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            CollectOperators();
            _virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            _thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
            InitActiveSlot();
        }

        //TODO:以后添加资源管理功能的时候，这里要读取队伍资源而不是读取场景
        private void CollectOperators()
        {
            _operators.Clear();
            foreach (var op in FindObjectsOfType<Operator>())
            {
                if (op.OperatorData != null && !_operators.ContainsKey(op.OperatorData))
                    _operators[op.OperatorData] = op;
            }
        }

        private void InitActiveSlot()
        {
            // 以"当前启用了 CharacterPlayerController 的干员"为默认主控
            for (int i = 0; i < SlotCount(); i++)
            {
                if (TryGetOperatorInSlot(i, out var op))
                {
                    var playerCtrl = op.GetComponent<CharacterPlayerController>();
                    if (playerCtrl != null && playerCtrl.enabled)
                    {
                        _activeSlot = i;
                        ActiveOperator = op;
                        return;
                    }
                }
            }
            _activeSlot = 0;
            ActiveOperator = GetOperatorInSlot(0);

            if (ActiveOperator != null)
                ReTargetCamera(ActiveOperator);   // 开局相机对齐当前主控
        }

        private int SlotCount() => team != null && team.slots != null ? team.slots.Count : 0;

        private bool TryGetOperatorInSlot(int slot, out Operator op)
        {
            op = GetOperatorInSlot(slot);
            return op != null;
        }

        /// <summary>取指定槽位（0~3）的干员；空槽/未在场景/未配置返回 null。未来技能键用。</summary>
        public Operator GetOperatorInSlot(int slot)
        {
            if (team == null || slot < 0 || slot >= team.slots.Count) return null;
            var so = team.slots[slot];
            if (so == null) return null;
            return _operators.TryGetValue(so, out var op) ? op : null;
        }

        /// <summary>顺序切人（Q）：切到下一个有干员的槽位。</summary>
        public void SwitchNext()
        {
            if (!CanSwitch()) return;
            if (SlotCount() == 0) return;

            for (int step = 1; step <= SlotCount(); step++)
            {
                int candidate = (_activeSlot + step) % SlotCount();
                if (TryGetOperatorInSlot(candidate, out _))
                {
                    SwitchTo(candidate);
                    return;
                }
            }
        }

        /// <summary>切到指定槽位：交换位置/朝向 + 交换控制权 + 重指相机。</summary>
        public void SwitchTo(int slot)
        {
            if (!CanSwitch()) return;
            if (ActiveOperator == null) return;
            var target = GetOperatorInSlot(slot);
            if (target == null || target == ActiveOperator) return;

            SwapTransforms(ActiveOperator, target);
            SwapControl(ActiveOperator, target);
            ReTargetCamera(target);

            _activeSlot = slot;
            ActiveOperator = target;
        }

        private bool CanSwitch()
        {
            return ActiveOperator == null || ActiveOperator.CanSwitchOut();
        }

        /// <summary>交换两个干员的位置与朝向。</summary>
        private void SwapTransforms(Operator a, Operator b)
        {
            // CharacterController 直接改 transform 不会同步内部位置，先禁用再换位再启用
            var aCC = a.GetComponent<CharacterController>();
            var bCC = b.GetComponent<CharacterController>();
            if (aCC != null) aCC.enabled = false;
            if (bCC != null) bCC.enabled = false;

            var aT = a.transform;
            var bT = b.transform;
            var pos = aT.position;
            var rot = aT.rotation;
            aT.position = bT.position;
            aT.rotation = bT.rotation;
            bT.position = pos;
            bT.rotation = rot;

            if (aCC != null) aCC.enabled = true;
            if (bCC != null) bCC.enabled = true;
        }

        /// <summary>旧主控变 AI，新目标变玩家。</summary>
        private void SwapControl(Operator oldActive, Operator newActive)
        {
            SetControl(oldActive, isPlayer: false);
            SetControl(newActive, isPlayer: true);
        }

        private void SetControl(Operator op, bool isPlayer)
        {
            var playerCtrl = op.GetComponent<CharacterPlayerController>();
            var aiCtrl = op.GetComponent<CharacterAIController>();
            var behaviorTree = op.GetComponent<BehaviorTree>();
            var navMeshAgent = op.GetComponent<NavMeshAgent>();

            if (isPlayer)
            {
                if (navMeshAgent != null) navMeshAgent.enabled = false;
                if (behaviorTree != null) behaviorTree.enabled = false;
                if (aiCtrl != null) aiCtrl.enabled = false;
                if (playerCtrl != null) playerCtrl.enabled = true;
            }
            else
            {
                if (playerCtrl != null) playerCtrl.enabled = false;
                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = true;
                    navMeshAgent.Warp(op.transform.position);   // 交换位置后校准 NavMesh
                    navMeshAgent.ResetPath();                  // 清掉旧路径，行为树会重新下指令
                }
                if (aiCtrl != null) aiCtrl.enabled = true;
                if (behaviorTree != null) behaviorTree.enabled = true;
            }
        }

        /// <summary>相机重指新主控：Follow/LookAt + ThirdPersonCamera 目标与角度。</summary>
        private void ReTargetCamera(Operator target)
        {
            var point = FindCameraBasePoint(target.transform);
            if (point == null) return;

            if (_virtualCamera != null)
            {
                _virtualCamera.Follow = point;
                //_virtualCamera.LookAt = point;
            }
            if (_thirdPersonCamera != null)
                _thirdPersonCamera.SetTarget(point);
        }

        private static Transform FindCameraBasePoint(Transform root)
        {
            foreach (Transform child in root)
            {
                //TODO:以后换个方式查找
                if (child.name == "CameraBasePoint")
                    return child;
            }
            return root;
        }
    }
}
