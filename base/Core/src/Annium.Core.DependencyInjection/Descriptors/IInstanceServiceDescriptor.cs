namespace Annium.Core.DependencyInjection
{
    public interface IInstanceServiceDescriptor : IServiceDescriptor
    {
        public object ImplementationInstance { get; }
    }
}