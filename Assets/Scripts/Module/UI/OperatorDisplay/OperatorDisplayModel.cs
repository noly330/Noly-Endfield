using System.Collections;
using System.Collections.Generic;
using Endfield.Core;
using Endfield.Data.Catalog;
using UnityEngine;

namespace Endfield.Module.UI
{
    public class OperatorDisplayModel : BaseModel
    {
        public int currentID{get;set;} = 101;
        public OperatorSO GetCurrentOperator() => OperatorCatalog.Get(currentID);
    }
}