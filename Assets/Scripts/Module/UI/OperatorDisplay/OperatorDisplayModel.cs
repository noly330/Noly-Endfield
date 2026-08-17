using System.Collections;
using System.Collections.Generic;
using Endfield.Core;
using Endfield.Data.Catalog;
using Endfield.Data.User;
using UnityEngine;

namespace Endfield.Module.UI
{
    public class OperatorDisplayModel : BaseModel
    {
        public int currentID{get;set;} = 101;
        public OperatorSO GetCurrentOperator() => OperatorCatalog.Get(currentID);
        public IReadOnlyList<int> OwnedIds => UserDataService.Instance.Current?.ownedOperatorIds;
        public Sprite GetAvatar(int id) => OperatorCatalog.Get(id)?.displayData?.avatar;
    }
}