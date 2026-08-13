using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Endfield.Core.Pool;
using Endfield.Module.VFX;
using UnityEngine;

namespace Endfield.Core.Resource
{
    /// <summary>
    /// 启动预热表：集中列出所有要在游戏启动时预热的对象池。
    /// 每条目是一个闭包，负责预热一个池（类型安全，无反射）。
    /// 加新预热 = 往 All 里加一行（顺序 = 预热顺序，先核心后次要）。
    /// </summary>
    public static class PrewarmRegister
    {
        public static readonly IReadOnlyList<Func<UniTask>> All = new Func<UniTask>[]
        {
            // 敌人池：加载 prefab + 预热 10 个实例
            () => PrefabPoolManager.Instance.GetPoolAsync<Enemy>(
                "Assets/Res/Prefab/Character/Enemy/怪兽.prefab", 10, 30),

            // 特效池
            () => PrefabPoolManager.Instance.GetPoolAsync<PooledVFX>(
                "Assets/Res/VFX/VFX_Klaus/Prefabs/Slash/FX_slash_15.prefab", 5, 20),
            () => PrefabPoolManager.Instance.GetPoolAsync<PooledVFX>(
                "Assets/Res/VFX/VFX_Klaus/Prefabs/Hit/FX_hit_spark.prefab",10,40),
             () => PrefabPoolManager.Instance.GetPoolAsync<PooledVFX>(
                "Assets/Res/VFX/VFX_Klaus/Prefabs/Hit/FX_hit_15.prefab",5,20)
            
        };

        /// <summary>遍历预热表，逐个预热。</summary>
        public static async UniTask PrewarmAll()
        {
            foreach (var entry in All)
            {
                await entry();
                Debug.Log("[Prewarm] 预热完成一项");
            }

            Debug.Log($"[Prewarm] 全部预热完成，共 {All.Count} 项");
        }
    }
}
