using System.Collections.Generic;
using UnityEngine;
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
    /// 连携子系统在 Links（连携队列/CD/打出/慢动作/镜头）。
    /// 由 GameLauncher 在玩家数据 + 干员图鉴就绪后调用 InitializeAsync。
    /// </summary>
    public class TeamManager : Singleton<TeamManager>
    {
        #region 字段与属性
        private readonly Dictionary<OperatorSO, Operator> _operators = new();
        private int _activeSlot;
        private ThirdPersonCamera _thirdPersonCamera;
        private Transform _root;

        /// <summary>当前玩家控制的干员。</summary>
        public Operator ActiveOperator { get; private set; }

        /// <summary>连携子系统（连携队列 / CD / 打出 / 慢动作 / 连携镜头）。</summary>
        public LinkSystem Links { get; private set; }

        /// <summary>已加载的干员数量（队伍人数）。</summary>
        public int TeamCount => _operators.Count;
        #endregion

        #region 队伍加载与初始化
        /// <summary>由 GameLauncher 在玩家数据 + 干员图鉴就绪后调用。</summary>
        public async UniTask InitializeAsync(Transform root, ThirdPersonCamera thirdPersonCamera)
        {
            _root = root;
            _thirdPersonCamera = thirdPersonCamera;
            Links = new LinkSystem(this, thirdPersonCamera);
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
                if (op != null) op.SetPlayerControl(i == 0);
            }

            _thirdPersonCamera?.FocusOn(ActiveOperator.transform);   // 镜头入口（实现在 ThirdPersonCamera）
        }

        private int SlotCount() => UserDataService.Instance.Current?.teamSlotIds?.Count ?? 0;
        #endregion

        #region 干员查找
        /// <summary>
        /// 按队伍索引取干员（1 = 第一位）
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

        /// <summary>干员 → 队伍槽位（0 基）；不在队返回 -1。</summary>
        public int GetSlotIndex(Operator op)
        {
            var ids = UserDataService.Instance.Current?.teamSlotIds;
            if (ids == null || op == null) return -1;
            for (int i = 0; i < ids.Count; i++)
                if (GetOperatorInSlot(i) == op) return i;
            return -1;
        }
        #endregion

        #region 切人
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
            _thirdPersonCamera?.FocusOn(target.transform);   // 镜头入口（实现在 ThirdPersonCamera）

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
            oldActive.SetPlayerControl(false);
            newActive.SetPlayerControl(true);
        }
        #endregion

        #region 技能释放
        /// <summary>
        /// 按队伍索引(1基)释放技能：主控直接放；非主控瞬移到主控缓存目标附近放。
        /// </summary>
        public void TryCastSkill(int index)
        {
            var op = GetOperatorByIndex(index);
            if (op == null || ActiveOperator == null) return;

            if (op == ActiveOperator)
            {
                op.combatDriver.skillAttack = true;
                return;
            }

            // 非主控：瞬移到主控缓存目标左右侧（按编队奇偶），面朝目标放技能
            var target = ActiveOperator.combatController.GetCurrentTarget();
            if (target == null) return;
            if (!TeleportToSlotSide(op, target, GetSlotIndex(op), 3f)) return;

            op.combatController.SetTarget(target);
            op.combatDriver.skillAttack = true;
        }
        #endregion

        #region 通用辅助
        /// <summary>
        /// 瞬移到目标敌人左右侧：以 主控干员(A)→目标敌人(B) 的水平线为基准，
        /// 过 B 点作垂线，干员落在垂线上（敌人左侧或右侧）并面朝敌人。
        /// 方向按编队位置（1基）奇偶：奇数=左，偶数=右。无目标/方向无效返回 false。
        /// 技能/连携共用（LinkSystem 也调它）。
        /// </summary>
        internal bool TeleportToSlotSide(Operator op, Transform target, int slotIndex, float distance)
        {
            if (target == null || ActiveOperator == null) return false;

            var toTarget = target.position - ActiveOperator.transform.position;   // 主控 → 敌人（水平）
            toTarget.y = 0;
            if (toTarget.sqrMagnitude < 0.0001f) return false;

            var side = Vector3.Cross(Vector3.up, toTarget.normalized);   // 过敌人的垂线方向（敌人左右）
            float dir = slotIndex < 0 || (slotIndex + 1) % 2 == 1 ? 1f : -1f;   // 奇数=左(+)，偶数=右(-)

            var pos = target.position + side * dir * distance;
            pos.y = op.transform.position.y;

            var faceDir = target.position - pos;
            faceDir.y = 0;
            op.TeleportTo(pos, Quaternion.LookRotation(faceDir));   // 瞬移下沉到 Character，本类不碰组件
            return true;
        }
        #endregion
    }
}
