using Cysharp.Threading.Tasks;
using Endfield.Core;
using Endfield.Core.Pool;
using Endfield.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 干员展示界面
    /// </summary>
    public class OperatorDisplayView : BaseView
    {
        private const string AvatarItemAddress = "Assets/Res/Prefab/UI/OperatorDisplay/AvatarItem.prefab";
        private const float AvatarItemSpacing = 12f;   // 格子间距

        private readonly OperatorDisplayModel _model;
        private RectTransform _starLevelRoot;
        private RectTransform _starSample;
        private VirtualScrollList<OperatorAvatarItem> _scrollList;
        private bool _escSubscribed;

        public OperatorDisplayView(OperatorDisplayModel model)
        {
            _model = model;
        }

        protected override async UniTask OnInit(Transform root)
        {
            _starLevelRoot = GetComponent<RectTransform>("Left/Header/StarLevel");
            if (_starLevelRoot != null && _starLevelRoot.childCount > 0)
            {
                _starSample = _starLevelRoot.GetChild(0).GetComponent<RectTransform>();
                _starSample.gameObject.SetActive(false);
            }
            Refresh();
            await InitAvatarList();
        }

        /// <summary>初始化顶部干员头像虚拟滚动列表。</summary>
        private async UniTask InitAvatarList()
        {
            var scroll = GetComponent<ScrollRect>("Top/ScrollView");
            if (scroll == null) return;

            var pool = await PrefabPoolManager.Instance.GetPoolAsync<OperatorAvatarItem>(AvatarItemAddress);
            if (pool == null) return;

            //格子步长 = 间距 + 量取的格子宽度
            float itemStep = AvatarItemSpacing + MeasureItemWidth(pool);
            _scrollList = new VirtualScrollList<OperatorAvatarItem>(scroll, itemStep, pool, (i, auatarItem) =>
            {
                //把显示干员的函数委托传进去
                int id = _model.OwnedIds[i];
                auatarItem.Setup(id, _model.GetAvatar(id), selected: id == _model.currentID, OnSelectAvatar);
            });
            _scrollList.SetCount(_model.OwnedIds?.Count ?? 0);
        }

        /// <summary>从格子 prefab 量取宽度（借一个实例量完即还）。</summary>
        private float MeasureItemWidth(PrefabPool<OperatorAvatarItem> pool)
        {
            var probe = pool.Get();
            float width = ((RectTransform)probe.transform).rect.width;
            pool.Release(probe);
            return width;
        }

        /// <summary>点击头像 → 切换当前干员 → 刷新左区 + 重跑可见格子更新高亮。</summary>
        private void OnSelectAvatar(int id)
        {
            _model.currentID = id;
            Refresh();
            _scrollList?.RebindVisible();
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
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            UnsubscribeEscape();
            PlayerInputSystem.Instance?.SetPlayerInputEnabled(true);    // 关闭 → 解冻
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _scrollList?.Dispose();
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
