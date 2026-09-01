using System.Collections.Generic;
using DG.Tweening;
using Endfield.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 连携队列头像 Widget（BaseWidget，挂在 CombatHUDView 的 LinkQueueContainer 下）。
    /// 订阅 Events.LinkSkillQueueChanged → 显示当前队列里能释放连携的干员头像。
    /// 固定 4 个槽位（Head0-3），只对新填入的干员播 pop-in；DOTween SetUpdate(true) 防慢动作。
    /// </summary>
    public class LinkQueueWidget : BaseWidget
    {
        protected override ViewEntry ViewEntry => UIRegister.LinkQueueWidget;

        private const float PopDuration = 0.18f;

        private readonly List<RectTransform> _heads = new();
        private readonly List<Image> _portraits = new();
        private readonly List<bool> _showing = new();

        protected override void OnInit()
        {
            base.OnInit();

            CollectHeads();
            EventCenter.SubscribeListener<Events.LinkSkillQueueChanged>(OnQueueChanged);
            Refresh();
            Show();   // 队列面板常驻显示（区别于连携横幅：无队列时为空、透明）
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventCenter.UnsubscribeListener<Events.LinkSkillQueueChanged>(OnQueueChanged);
        }

        /// <summary>收集预制体里的固定槽位（直接子节点，层级顺序 = 队列顺序）。</summary>
        private void CollectHeads()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i) is not RectTransform head) continue;
                var portrait = head.Find("Image")?.GetComponent<Image>();
                if (portrait == null) continue;

                _heads.Add(head);
                _portraits.Add(portrait);
                _showing.Add(false);
                head.gameObject.SetActive(false);
            }
        }

        private void OnQueueChanged(Events.LinkSkillQueueChanged _) => Refresh();

        private void Refresh()
        {
            var slots = TeamManager.Instance?.Links?.GetQueuedSlots();
            int count = slots?.Count ?? 0;

            for (int i = 0; i < _heads.Count; i++)
            {
                if (i < count)
                {
                    var op = TeamManager.Instance.GetOperatorInSlot(slots[i]);
                    if (op?.OperatorData?.displayData?.avatar is { } sprite)
                        _portraits[i].sprite = sprite;

                    if (_showing[i]) continue;   // 已在显示，保持

                    _heads[i].gameObject.SetActive(true);
                    PopIn(_heads[i]);
                    _showing[i] = true;
                }
                else
                {
                    _heads[i].gameObject.SetActive(false);
                    _showing[i] = false;
                }
            }
        }

        private void PopIn(RectTransform head)
        {
            head.localScale = Vector3.zero;
            head.DOScale(Vector3.one, PopDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }
}
