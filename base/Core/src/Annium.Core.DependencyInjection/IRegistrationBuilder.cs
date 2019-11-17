using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection
{
    public interface IRegistrationBuilder
    {
        IRegistrationBuilder Where(Func<Type, bool> predicate);
        IRegistrationBuilder AsImplementedInterfaces();
        void RegisterScoped();
        void RegisterSingleton();
        void RegisterTransient();
    }
}