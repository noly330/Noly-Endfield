using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;
using BehaviorDesigner.Runtime;
using Endfield.Core;
using Cysharp.Threading.Tasks;

namespace Endfield
{
    /// <summary>
    /// 队伍管理器：负责队伍组合、当前主控干员、切人（交换位置 + 控制权 + 相机）。
    /// 干员已改为按 TeamSO.slots 从 Addressables 加载，不再依赖场景摆放。
    /// TODO:队伍管理器目前仍有一些逻辑问题，待排查。
    /// </summary>
    public class TeamManager : SingletonMono<TeamManager>
    {
        [Tooltip("队伍配置（槽位 1~4，只放干员）")]
        public TeamSO team;

        private readonly Dictionary<OperatorSO, Operator> _operators = new Dictionary<OperatorSO, Operator>();
        private int _activeSlot;
        private CinemachineVirtualCamera _virtualCamera;
        private ThirdPersonCamera _thirdPersonCamera;

        /// <summary>当前玩家控制的干员。</summary>
        public Operator ActiveOperator { get; private set; }

        /// <summary>场景级服务，不跨场景常驻。</summary>
        protected override bool KeepAcrossScenes => false;

        protected override void Awake() => base.Awake();

        private async void Start()
        {
            await LoadTeamAsync();
            _virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            _thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
            InitActiveSlot();
        }

        /// <summary>
        /// 从 Addressables 按队伍配置（TeamSO.slots）加载干员，替换原来的场景扫描。
        /// 约定：干员 prefab 路径 = Assets/Res/Prefab/Character/Operator/{OperatorSO.name}.prefab
        /// </summary>
        private async UniTask LoadTeamAsync()
        {
            _operators.Clear();
            if (team == null)
            {
                Debug.LogWarning("[TeamManager] team 未配置，无法加载队伍");
                return;
            }

            foreach (var so in team.slots)
            {
                if (so == null) continue;

                string path = $"Assets/Res/Prefab/Character/Operator/{so.name}.prefab";
                var prefab = await ResourcesLoader.Instance.Load<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"[TeamManager] 加载干员 prefab 失败: {path}");
                    continue;
                }

                var opGo = Object.Instantiate(prefab, transform);
                opGo.name = so.name;
                var op = opGo.GetComponent<Operator>();
                if (op == null)
                {
                    Object.Destroy(opGo);
                    continue;
                }

                _operators[so] = op;
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
