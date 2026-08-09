using VContainer;

namespace Endfield.Core
{
    public class ModelManager : Singleton<ModelManager>
    {
        [Inject]
        private IObjectResolver Container{get;set;}

        public T Get<T>() where T : BaseModel
        {
            return Container.Resolve<T>();
        }
    }
}