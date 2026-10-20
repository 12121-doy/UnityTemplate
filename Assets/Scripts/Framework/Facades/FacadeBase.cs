using Guisha.Core;
namespace Guisha.Framework
{
    public abstract class FacadeBase
    {
        protected void Send<T>(T request) => EventBus.Publish(request);
    }
}
