using System;

namespace Annium.Core.DependencyInjection
{
    public interface IRegistrationBuilder
    {
        IRegistrationBuilder Where(Func<Type, bool> predicate);
        IRegistrationBuilder AsSelf();
        IRegistrationBuilder AsImplementedInterfaces();
        void InstancePerScope();
        void SingleInstance();
        void InstancePerDependency();
    }
}