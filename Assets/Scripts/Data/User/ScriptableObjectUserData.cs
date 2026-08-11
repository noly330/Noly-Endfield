using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Endfield.Data.User
{

    /// <summary>
    /// 开发期数据源：SO 实现 IUserDataProvider。
    /// SO 是"数据源"，不是"活模型"——Load 深拷贝给运行时，Save 写回资产。
    /// 以后接数据库 = 换成 NetworkUserDataProvider，本类退役。
    /// </summary>
    [CreateAssetMenu(menuName = "Endfield/Data/UserData")]

    public class ScriptableObjectUserData : ScriptableObject, IUserDataProvider
    {
        [SerializeField] private UserData _data = new();
        public UniTask<UserData> LoadAsync()
        {
            // 深拷贝：JSON 往返克隆，游戏逻辑改的是副本，不污染资产
            var copy = JsonUtility.FromJson<UserData>(JsonUtility.ToJson(_data));
            return UniTask.FromResult(copy);
        }

        public UniTask SaveAsync(UserData data)
        {
            _data = JsonUtility.FromJson<UserData>(JsonUtility.ToJson(data));
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);   // 通知编辑器资产被改了，能保存
#endif
            return UniTask.CompletedTask;
        }
    }
}