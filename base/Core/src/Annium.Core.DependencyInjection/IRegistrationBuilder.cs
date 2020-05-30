using System;

namespace Annium.Core.DependencyInjection
{
    public interface IRegistrationBuilder
    {
        IRegistrationBuilder Where(Func<Type, bool> predicate);
        IRegistrationBuilder As<T>();
        IRegistrationBuilder As(Type serviceType);
        IRegistrationBuilder AsSelf();
        IRegistrationBuilder AsImplementedInterfaces();
        void InstancePerScope();
        void SingleInstance();
        void InstancePerDependency();
    }
}