using Cysharp.Threading.Tasks;
using Endfield.Core;
using Endfield.Core.Pool;
using UnityEngine;
using UnityEngine.AI;

namespace Endfield
{
    /// <summary>
    /// 敌人刷怪服务：从 PrefabPool 生成敌人，死亡动画播完自动回收。
    /// Character 不碰对象池——谁生成谁回收（SRP）。
    /// </summary>
    public class EnemySpawner : Singleton<EnemySpawner>
    {
        /// <summary>
        /// 从对象池生成敌人并自动接线"死亡动画播完→回收"（首次调用会异步加载 prefab）。
        /// </summary>
        public async UniTask<Enemy> SpawnAsync(string prefabPath, Vector3 position, Quaternion rotation)
        {
            var pool = await PrefabPoolManager.Instance.GetPoolAsync<Enemy>(prefabPath);
            var enemy = pool.Get();   // SetActive(true) → Enemy.OnEnable 自动重置
            enemy.transform.SetPositionAndRotation(position, rotation);

            // 强制内部状态同步到出生点，否则会被拖回旧位置
            var cc = enemy.GetComponent<CharacterController>();
            if (cc != null) { cc.enabled = false; cc.enabled = true; }   // 胶囊重同步到 transform
            var nav = enemy.GetComponent<NavMeshAgent>();
            if (nav != null) { nav.Warp(position); nav.ResetPath(); }     // agent 定位到出生点、清旧路径

            void HandleDeath()
            {
                enemy.OnDeathAnimationEnd -= HandleDeath;   // 防重复回收
                pool.Release(enemy);
            }
            enemy.OnDeathAnimationEnd += HandleDeath;

            return enemy;
        }
    }
}
