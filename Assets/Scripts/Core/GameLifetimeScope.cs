using System.Reflection;
using Endfield.Data.User;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Endfield.Core
{

    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private ScriptableObjectUserData userData;
        protected override void Configure(IContainerBuilder builder)
        {

            builder.RegisterInstance<IUserDataProvider>(userData);
            //Assembly.GetExecutingAssembly().GetTypes()：获取当前程序集中所有的类型
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                //注册所有 BaseModel 子类，排除抽象类（因为不能实例化）
                if (type.IsSubclassOf(typeof(BaseModel)) && !type.IsAbstract)
                {
                    // 1. 反射扫描：所有 BaseModel 子类自动注册为单例
                    builder.Register(type, Lifetime.Singleton).AsSelf();
                }
            }

            //注入所有UI视图
            foreach (var ve in UIRegister.All)
            {
                // 2. 所有 UI 视图注册为瞬时（每次 Resolve 都是新实例）
                if (ve.ViewType != null)
                    builder.Register(ve.ViewType, Lifetime.Transient);
            }

            //注入的 IObjectResolver 就是 VContainer 容器本身——它的作用是"工厂 + 仓库管理员"。
            //它只提供两个能力
            //Container.Resolve(type)   // ① "给我造一个这个类型的实例，把它的依赖也装好"
            //Container.Inject(instance) // ② "给这个已存在的实例补依赖
            builder.RegisterBuildCallback(container =>
            {
                // 3. 对"不是容器创建的"单例，用 Inject 补依赖
                container.Inject(UIManager.Instance);
                container.Inject(ModelManager.Instance);
                container.Inject(UserDataService.Instance);
            });
        }
    }
}

//UI注入时间线
//GameLauncher.Start
//  → UIManager.Instance.OpenView(MainView, ...)
//  → 加载 prefab、Instantiate 到画布
// → Container.Resolve(typeof(MainView))     ← 注入发生在这里！
//    容器内部：
//   ① new MainView()（无参构造，此刻 Container 还是 null）
//   ② 扫描所有可注入成员（含继承自 BaseView 的）
//     ③ 发现 [Inject] IObjectResolver Container
//    ④ 把容器引用赋给该属性            ← 注入完成
// → 返回 viewInstance（Container 已可用）
// → viewInstance.Initialize(...)         ← 注入早已结束
