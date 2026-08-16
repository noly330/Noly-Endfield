using Cysharp.Threading.Tasks;
using Endfield.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 干员展示界面：左侧展示干员名字/星级/基础属性。
    /// 数据来自 OperatorDisplayModel（当前干员）→ OperatorSO → displayData/attributeData。
    /// </summary>
    public class OperatorDisplayView : BaseView
    {
        private readonly OperatorDisplayModel _model;
        private RectTransform _starLevelRoot;
        private RectTransform _starSample;
        private bool _escSubscribed;

        public OperatorDisplayView(OperatorDisplayModel model)
        {
            _model = model;
        }

        protected override UniTask OnInit(Transform root)
        {
            _starLevelRoot = GetComponent<RectTransform>("Left/Header/StarLevel");
            if (_starLevelRoot != null && _starLevelRoot.childCount > 0)
            {
                _starSample = _starLevelRoot.GetChild(0).GetComponent<RectTransform>();
                _starSample.gameObject.SetActive(false);
            }
            Refresh();
            return UniTask.CompletedTask;
        }

        private void Refresh()
        {
            var so = _model.GetCurrentOperator();
            if (so == null) return;

            var displayData = so.displayData;
            GetComponent<TextMeshProUGUI>("Left/Header/Name").text
                = displayData != null && !string.IsNullOrEmpty(displayData.name) ? displayData.name : so.name;

            var attr = so.attributeData;
            GetComponent<TextMeshProUGUI>("Left/Attribute/BaseAttribute/Health/Value").text = attr.maxHp.ToString();
            GetComponent<TextMeshProUGUI>("Left/Attribute/BaseAttribute/Attack/Value").text = attr.atk.ToString();
            GetComponent<TextMeshProUGUI>("Left/Attribute/BaseAttribute/Defense/Value").text = attr.def.ToString();

            SetStars(displayData != null ? displayData.starLevel : 0);
        }

        protected override void OnShown(object data)
        {
            base.OnShown(data);
            if (PlayerInputSystem.Instance != null)
            {
                PlayerInputSystem.Instance.CloseView.performed += OnEscape;
                _escSubscribed = true;
            }
            PlayerInputSystem.Instance?.SetPlayerInputEnabled(false);   // 打开 → 冻结角色输入
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            UnsubscribeEscape();
            PlayerInputSystem.Instance?.SetPlayerInputEnabled(true);    // 关闭 → 解冻
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeEscape();
            PlayerInputSystem.Instance?.SetPlayerInputEnabled(true);
        }

        private void UnsubscribeEscape()
        {
            if (_escSubscribed && PlayerInputSystem.Instance != null)
                PlayerInputSystem.Instance.CloseView.performed -= OnEscape;
            _escSubscribed = false;
        }

        private void OnEscape(InputAction.CallbackContext _)
            => UIManager.Instance.CloseView(UIRegister.OperatorDisplayView);

        /// <summary>
        /// 星级显示：隐藏 sample 模板，按星级复制 starLevel 个星图标。
        /// </summary>
        private void SetStars(int starLevel)
        {
            if (_starSample == null) return;

            for (int i = _starLevelRoot.childCount - 1; i >= 0; i--)
            {
                if (_starLevelRoot.GetChild(i) != _starSample)
                    Object.Destroy(_starLevelRoot.GetChild(i).gameObject);
            }

            for (int i = 0; i < starLevel; i++)
                Object.Instantiate(_starSample, _starLevelRoot).gameObject.SetActive(true);
        }
    }
}
