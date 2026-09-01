using DG.Tweening;
using Endfield.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 连携技左侧照片横幅（BaseWidget，挂在 CombatHUDView 下）。
    /// 订阅 Events.LinkSkillCast → 从左侧滑入显示干员连携照片 → 停留 → 自动隐藏。
    /// 用 DOTween Sequence；SetUpdate(true) 不受连携技慢动作（TimeDirector.SlowTo）影响。
    /// </summary>
    public class LinkBannerWidget : BaseWidget
    {
        protected override ViewEntry ViewEntry => UIRegister.LinkBannerWidget;

        private RectTransform _icon;
        private Image _image;
        private Vector2 _targetPosition;
        private Sequence _showSequence;

        [SerializeField] private float enterDuration = 0.4f;
        [SerializeField] private float stayDuration = 1f;
        [SerializeField] private float outsideOffsetX = 900f;

        protected override void OnInit()
        {
            base.OnInit();

            _icon = GetComponent<RectTransform>("Main");
            _image = GetComponent<Image>("Main/Image");
            if (_icon != null) _targetPosition = _icon.anchoredPosition;

            EventCenter.SubscribeListener<Events.LinkSkillCast>(OnLinkSkill);
        }

        protected override void OnShown(object data)
        {
            base.OnShown(data);
            if (data is Sprite sprite && _image != null)
            {
                _image.sprite = sprite;
                PlayShow();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventCenter.UnsubscribeListener<Events.LinkSkillCast>(OnLinkSkill);
            _showSequence?.Kill();
        }

        private void OnLinkSkill(Events.LinkSkillCast msg)
        {
            Show(msg.linkHead);
        }

        private void PlayShow()
        {
            // 防止连携连发时多个动画同时控制同一个 UI
            _showSequence?.Kill();

            if (_icon == null) return;
            Vector2 start = _targetPosition + Vector2.left * outsideOffsetX;
            _icon.anchoredPosition = start;

            _showSequence = DOTween.Sequence()
                .SetUpdate(true)   // 用真实时间，不受 Time.timeScale 慢动作影响
                .Append(_icon.DOAnchorPos(_targetPosition, enterDuration).SetEase(Ease.OutCubic))
                .AppendInterval(stayDuration)
                .AppendCallback(() => gameObject.SetActive(false))
                .OnComplete(() => _showSequence = null);
        }
    }
}
