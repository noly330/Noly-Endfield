using System.Collections.Generic;
using Endfield.Core;

namespace Endfield
{
    /// <summary>
    /// 连携子系统（纯 C#）：连携队列 / CD 门 / 打出 / 慢动作 / 连携镜头。
    /// 协作：经 TeamManager 拿队伍（槽位干员/主控/站位助手），经 ThirdPersonCamera 调连携镜头。
    /// 由 TeamManager 创建并暴露（TeamManager.Links）。
    /// </summary>
    public class LinkSystem
    {
        private readonly TeamManager _team;
        private readonly ThirdPersonCamera _camera;
        private readonly Queue<int> _linkQueue = new();       // 连携队列（槽位 0 基）
        private readonly HashSet<int> _linkQueuedSlots = new();   // 防重

        public LinkSystem(TeamManager team, ThirdPersonCamera camera)
        {
            _team = team;
            _camera = camera;
        }

        /// <summary>连携入队：连携 CD 已好（LinkReady）+ 未在队 + 队未满（&lt;4）。</summary>
        public void TryEnqueueLinkAttack(Operator op)
        {
            if (op == null) return;
            int slot = _team.GetSlotIndex(op);
            if (slot < 0) return;
            if (!op.LinkReady) return;
            if (_linkQueuedSlots.Contains(slot)) return;
            if (_linkQueue.Count >= 4) return;
            _linkQueue.Enqueue(slot);
            _linkQueuedSlots.Add(slot);
        }

        /// <summary>
        /// 打出队首干员的连携（Link 键）：主控直接放；非主控瞬移到主控缓存目标左右侧放。
        /// 队首忙（战技/受击/死亡中）保持排队等下次按键；打出后出队 + 重置该干员连携 CD + 慢动作 + 连携镜头 + 广播连携链事件。
        /// </summary>
        public void TryCastLink()
        {
            if (_linkQueue.Count == 0) return;
            int slot = _linkQueue.Peek();
            var op = _team.GetOperatorInSlot(slot);
            if (op == null || op.LinkAttackData == null)   // 无效队首直接丢弃，避免卡队列
            {
                _linkQueue.Dequeue();
                _linkQueuedSlots.Remove(slot);
                return;
            }
            if (!op.CanCastLink()) return;                  // 忙：等下次按键

            if (op != _team.ActiveOperator)
            {
                // 非主控：瞬移到主控缓存目标左右侧（按编队奇偶）并指目标
                var target = _team.ActiveOperator?.combatController.GetCurrentTarget();
                if (target == null) return;                 // 无目标保持排队
                if (!_team.TeleportToSlotSide(op, target, slot, 3f)) return;
                op.combatController.SetTarget(target);
            }

            _linkQueue.Dequeue();
            _linkQueuedSlots.Remove(slot);
            op.combatDriver.linkAttack = true;
            op.ResetLinkCooldown();                          // 出队时重置连携 CD

            TimeDirector.SlowTo(0.25f, 0.8f);                 // 连携慢动作：时间缩到 0.25，持续 0.8s
            if (op != _team.ActiveOperator && _camera != null)
                _camera.LinkFocusOn(op.transform, 0.8f);     // 连携镜头：到位 → 固定 → 结束立刻回

            EventCenter.DispatchMessage(new Events.OnLinkSkillTriggered());   // 连携链
        }
    }
}
