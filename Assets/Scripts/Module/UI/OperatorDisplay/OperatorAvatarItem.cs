using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 干员头像格子：挂在 AvatarItem prefab 上（Button + 头像 Image + 选中框 Image）。
    /// 池化复用：Setup 设置数据/回调，Clear 清回调防泄漏。
    /// </summary>
    public class OperatorAvatarItem : MonoBehaviour
    {
        [SerializeField] private Image _avatar;
        [SerializeField] private Image _selectedBorder;

        private Button _button;
        private Action<int> _onClick;
        private int _id;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        /// <summary>设置格子数据。selected 为 true 时显示选中框。</summary>
        public void Setup(int id, Sprite avatar, bool selected, Action<int> onClick)
        {
            _id = id;
            if (_avatar != null) _avatar.sprite = avatar;
            if (_selectedBorder != null) _selectedBorder.gameObject.SetActive(selected);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
            _onClick = onClick;
        }

        /// <summary>归还对象池前调用：清回调，防泄漏。</summary>
        public void Clear()
        {
            _onClick = null;
            if (_button != null) _button.onClick.RemoveAllListeners();
        }

        private void OnClick() => _onClick?.Invoke(_id);
    }
}
