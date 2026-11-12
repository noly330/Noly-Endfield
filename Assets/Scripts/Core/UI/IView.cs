using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Endfield.Core
{
    public interface IView
    {
        /// <summary>
        /// 初始化视图，并接管由 UIManager 创建的资源作用域。
        /// scope 为 null 时（例如单独 new 出来的 Widget）由实现方自行创建。
        /// </summary>
        /// <param name="root">视图根节点</param>
        /// <param name="scope">资源作用域</param>
        UniTask Initialize(Transform root,ResourceScope scope = null);

        void Show(object data = null);

        void Hide();

        void Destroy();
        
    }
}