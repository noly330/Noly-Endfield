using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Endfield.Core
{
    public interface IView
    {
        /// <summary>
        /// 初始化，传入视图的根Transform
        /// </summary>
        /// <param name="root"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        UniTask Initialize(Transform root,string viewName = "");

        void Show(object data = null);

        void Hide();

        void Destroy();
        
    }
}