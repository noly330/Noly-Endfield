using System.Reflection;
using VContainer;
using VContainer.Unity;


namespace Endfield.Core
{

    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            //Assembly.GetExecutingAssembly().GetTypes()：获取当前程序集中所有的类型
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                //注册所有 BaseModel 子类，排除抽象类（因为不能实例化）
                if (type.IsSubclassOf(typeof(BaseModel)) && !type.IsAbstract)
                {
                    builder.Register(type, Lifetime.Singleton).AsSelf();
                }
            }

            //注入所有UI视图
            foreach (var ve in UIRegister.All)
            {
                if (ve.ViewType != null)
                    builder.Register(ve.ViewType, Lifetime.Transient);
            }

            //注入的 IObjectResolver 就是 VContainer 容器本身——它的作用是"工厂 + 仓库管理员"。
            //它只提供两个能力
            //Container.Resolve(type)   // ① "给我造一个这个类型的实例，把它的依赖也装好"
            //Container.Inject(instance) // ② "给这个已存在的实例补依赖
            builder.RegisterBuildCallback(container =>
            {
                container.Inject(UIManager.Instance);
                container.Inject(ModelManager.Instance);
            });
        }
    }
}
