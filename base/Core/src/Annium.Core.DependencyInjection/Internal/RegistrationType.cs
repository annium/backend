using System;

namespace Annium.Core.DependencyInjection.Internal
{
    [Flags]
    internal enum RegistrationType
    {
        Self = 0,
        ImplementedInterfaces = 1,
    }
}