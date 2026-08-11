using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using Endfield.Core;
using Cysharp.Threading.Tasks;
using Endfield.Data.User;
using Endfield.Data.Catalog;

namespace Endfield
{
    /// <summary>
    /// 队伍管理器（纯 C# 单例）：读取玩家数据（UserData.teamSlotIds）加载队伍。
    /// 槽 0（队伍第一位）为主控干员，其余为 AI。
    /// 提供按队伍索引（1=第一位）获取干员的接口，供技能释放使用。
    /// 由 GameLauncher 在玩家数据 + 干员图鉴就绪后调用 InitializeAsync。
    /// </summary>
    public class TeamManager : Singleton<TeamManager>
    {
        private readonly Dictionary<OperatorSO, Operator> _operators = new();
        private int _activeSlot;
        private ThirdPersonCamera _thirdPersonCamera;
        private Transform _root;

        /// <summary>当前玩家控制的干员。</summary>
        public Operator ActiveOperator { get; private set; }

        /// <summary>已加载的干员数量（队伍人数）。</summary>
        public int TeamCount => _operators.Count;

        /// <summary>
        /// 按队伍索引取干员（1 = 第一位）。技能释放用，越界返回 null。
        /// </summary>
        public Operator GetOperatorByIndex(int index)
        {
            if (index < 1) return null;
            return GetOperatorInSlot(index - 1);
        }

        /// <summary>取指定槽位（0~N-1）的干员；空/未加载返回 null。</summary>
        public Operator GetOperatorInSlot(int slot)
        {
            var ids = UserDataService.Instance.Current?.teamSlotIds;
            if (ids == null || slot < 0 || slot >= ids.Count) return null;
            var so = OperatorCatalog.Get(ids[slot]);
            return _operators.TryGetValue(so, out var op) ? op : null;
        }

        /// <summary>由 GameLauncher 在玩家数据 + 干员图鉴就绪后调用。</summary>
        public async UniTask InitializeAsync(Transform root, ThirdPersonCamera thirdPersonCamera)
        {
            _root = root;
            _thirdPersonCamera = thirdPersonCamera;
            await LoadTeamAsync();
            InitActiveSlot();
        }

        /// <summary>按玩家编队（UserData.teamSlotIds）从干员图鉴加载干员。</summary>
        private async UniTask LoadTeamAsync()
        {
            _operators.Clear();

            var teamSlotIds = UserDataService.Instance.Current?.teamSlotIds;
            if (teamSlotIds == null || teamSlotIds.Count == 0)
            {
                Debug.LogWarning("[TeamManager] 编队为空，无法加载队伍");
                return;
            }

            foreach (var id in teamSlotIds)
            {
                var so = OperatorCatalog.Get(id);
                if (so == null)
                {
                    Debug.LogWarning($"[TeamManager] 干员 id={id} 不在图鉴中");
                    continue;
                }
                if (string.IsNullOrEmpty(so.prefabAddress))
                {
                    Debug.LogWarning($"[TeamManager] 干员 {so.name} 未配置 prefabAddress");
                    continue;
                }

                var prefab = await ResourcesLoader.Instance.Load<GameObject>(so.prefabAddress);
                if (prefab == null)
                {
                    Debug.LogError($"[TeamManager] 加载干员 prefab 失败: {so.prefabAddress}");
                    continue;
                }

                var opGo = Object.Instantiate(prefab, _root);
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

        /// <summary>队伍第一位为主控，其余切 AI；相机指向主控。</summary>
        private void InitActiveSlot()
        {
            _activeSlot = 0;
            ActiveOperator = GetOperatorInSlot(0);
            if (ActiveOperator == null) return;

            // 槽 0 = 主控，其余 = AI（否则加载的 prefab 全员吃玩家输入）
            for (int i = 0; i < SlotCount(); i++)
            {
                var op = GetOperatorInSlot(i);
                if (op != null) SetControl(op, isPlayer: i == 0);
            }

            ReTargetCamera(ActiveOperator);
        }

        private int SlotCount() => UserDataService.Instance.Current?.teamSlotIds?.Count ?? 0;

        /// <summary>顺序切人（Q）：切到下一个有干员的槽位。</summary>
        public void SwitchNext()
        {
            if (!CanSwitch()) return;
            if (SlotCount() == 0) return;

            for (int step = 1; step <= SlotCount(); step++)
            {
                int candidate = (_activeSlot + step) % SlotCount();
                if (GetOperatorInSlot(candidate) != null)
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

        private bool CanSwitch() => ActiveOperator == null || ActiveOperator.CanSwitchOut();

        /// <summary>交换两个干员的位置与朝向。</summary>
        private void SwapTransforms(Operator a, Operator b)
        {
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
                    navMeshAgent.Warp(op.transform.position);
                    navMeshAgent.ResetPath();
                }
                if (aiCtrl != null) aiCtrl.enabled = true;
                if (behaviorTree != null) behaviorTree.enabled = true;
            }
        }

        /// <summary>相机重指主控：只调 ThirdPersonCamera 的统一入口。</summary>
        private void ReTargetCamera(Operator target)
        {
            var point = FindCameraBasePoint(target.transform);
            if (point == null) return;
            if (_thirdPersonCamera != null)
                _thirdPersonCamera.FollowTarget(point);
        }

        private static Transform FindCameraBasePoint(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.name == "CameraBasePoint")
                    return child;
            }
            return root;
        }
    }
}
