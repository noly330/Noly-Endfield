using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Endfield.Core;
using VContainer;

namespace Endfield.Data.User
{
    /// <summary>
    /// 玩家数据运行时管理器：持有当前 UserData，业务唯一入口。
    /// Provider 由 VContainer 注入（SO 或未来的网络实现）。
    /// </summary>
    public class UserDataService : Singleton<UserDataService>
    {
        [Inject] private IUserDataProvider Provider { get; set; }

        public UserData Current { get; private set; }

        /// <summary>启动时加载账号数据。</summary>
        public async UniTask InitializeAsync()
        {
            Current = await Provider.LoadAsync();
        }

        public void AddOwnedOperator(int operatorId)
        {
            if (!Current.ownedOperatorIds.Contains(operatorId))
                Current.ownedOperatorIds.Add(operatorId);
        }

        public void SetTeam(List<int> slotIds)
        {
            Current.teamSlotIds.Clear();
            Current.teamSlotIds.AddRange(slotIds);
        }

        public async UniTask SaveAsync() => await Provider.SaveAsync(Current);
    }
}
