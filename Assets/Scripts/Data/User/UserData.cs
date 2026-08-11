using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Endfield.Data.User
{
    /// <summary>
    /// 账号级玩家数据 DTO：纯数据模型，只存 ID，可序列化。为未来的数据扩展预留空间。
    /// </summary>
    [Serializable]
    public class UserData
    {
        public int version = 1;                    // 存档版本
        public List<int> ownedOperatorIds = new(); // 拥有的干员
        public List<int> teamSlotIds = new();      // 当前编队 1-4
    }

    /// <summary>
    /// 玩家数据访问接口：业务只依赖它，不依赖具体实现（DIP/OCP）。
    /// 现在：ScriptableObjectUserData；以后：NetworkUserDataProvider（数据库/服务器）。
    /// </summary>
    public interface IUserDataProvider
    {
        UniTask<UserData> LoadAsync();    // 加载账号数据
        UniTask SaveAsync(UserData data); // 保存账号数据
    }



}