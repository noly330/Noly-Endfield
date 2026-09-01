using Cysharp.Threading.Tasks;
using DG.Tweening;
using Endfield.Core;
using Endfield.Core.Pool;
using TMPro;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 伤害飘字系统（纯 C# 单例）：订阅 Events.OnCharacterDamaged，
    /// 把受击世界坐标投影到屏幕，在 UI 层用对象池弹一个数字，上浮 + 淡出后归还。
    /// 普通=白色，暴击=红色且更大；SetUpdate(true) 不受战斗慢动作影响。
    /// </summary>
    public class DamageTextSystem : Singleton<DamageTextSystem>
    {
        private const string DamageTextAddress = "Assets/Res/Prefab/UI/CombatHUDView/DamageText.prefab";

        private const float NormalSize = 36f;
        private const float CritSize = 52f;
        private const float FloatDistance = 80f;   // 上浮像素
        private const float Duration = 0.55f;      // 上浮+淡出总时长
        private static readonly Color NormalColor = Color.white;
        private static readonly Color CritColor = new Color(1f, 0.25f, 0.25f);

        private RectTransform _container;
        private PrefabPool<TextMeshProUGUI> _pool;
        private Camera _camera;
        private bool _initialized;

        public async UniTask InitializeAsync(RectTransform container)
        {
            _container = container;

            // 缓存主相机一次（Camera.main 内部按 tag 查找，别每次命中都查）
            _camera = Camera.main;

            _pool = await PrefabPoolManager.Instance.GetPoolAsync<TextMeshProUGUI>(DamageTextAddress,100,200);
            if (_pool == null)
            {
                Debug.LogError($"[DamageTextSystem] 加载伤害飘字预制体失败: {DamageTextAddress}");
                return;
            }

            EventCenter.SubscribeListener<Events.OnCharacterDamaged>(OnDamaged);
            _initialized = true;
        }

        private void OnDamaged(Events.OnCharacterDamaged msg)
        {
            if (!_initialized || _pool == null) return;
            Spawn(msg);
        }

        private void Spawn(Events.OnCharacterDamaged msg)
        {
            var text = _pool.Get();
            var rt = text.rectTransform;
            rt.SetParent(_container, false);

            // 清除上次残留的动画，避免新旧 Tween 抢同一个对象
            text.DOKill();
            rt.DOKill();

            // 世界命中点 → 屏幕坐标 → 容器局部坐标（发飘字停在命中点附近）
            if (_camera == null) { _pool.Release(text); return; }
            Vector2 screen = _camera.WorldToScreenPoint(msg.hitPos);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, screen, null, out Vector2 local))
                rt.anchoredPosition = local;

            text.raycastTarget = false;
            text.text = Mathf.RoundToInt(msg.damage).ToString();
            text.color = msg.isCrit ? CritColor : NormalColor;
            text.fontSize = msg.isCrit ? CritSize : NormalSize;
            text.gameObject.SetActive(true);

            // 上浮 + 淡出（真实时间，防慢动作）
            text.DOFade(0f, Duration).SetUpdate(true).SetEase(Ease.OutQuad);
            rt.DOAnchorPos(rt.anchoredPosition + Vector2.up * FloatDistance, Duration)
              .SetUpdate(true).SetEase(Ease.OutQuad)
              .OnComplete(() => { if (text != null) _pool.Release(text); });
        }

        public void Dispose()
        {
            EventCenter.UnsubscribeListener<Events.OnCharacterDamaged>(OnDamaged);
            _initialized = false;
        }
    }
}
