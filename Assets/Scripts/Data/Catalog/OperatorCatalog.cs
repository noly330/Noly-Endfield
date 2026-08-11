using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Endfield.Data.Catalog
{
    /// <summary>
    /// 干员图鉴：id → OperatorSO。从 Addressables 按 "Operator" label 构建。
    /// 新增干员 = 建 SO + 设 ID + 打 label，自动入库。
    /// </summary>
    public static class OperatorCatalog
    {
        private const string OperatorLabel = "Operator";

        private static readonly Dictionary<int, OperatorSO> _map = new();

        /// <summary>图鉴是否已构建完成（TeamManager 等它再查）。</summary>
        public static bool IsBuilt { get; private set; }

        /// <summary>启动时构建一次：扫描 label 加载所有 OperatorSO，建 id → SO 映射。</summary>
        public static async UniTask BuildAsync()
        {
            _map.Clear();
            IsBuilt = false;

            var locations = await Addressables.LoadResourceLocationsAsync(OperatorLabel, typeof(OperatorSO)).Task;
            foreach (var loc in locations)
            {
                var so = await Addressables.LoadAssetAsync<OperatorSO>(loc).Task;
                if (so != null && so.ID > 0)
                    _map[so.ID] = so;
            }

            IsBuilt = true;
        }

        /// <summary>按 id 查干员；查不到返回 null。</summary>
        public static OperatorSO Get(int id)
            => _map.TryGetValue(id, out var so) ? so : null;

        /// <summary>所有干员（图鉴 UI 用）。</summary>
        public static IReadOnlyCollection<OperatorSO> All => _map.Values;
    }
}
