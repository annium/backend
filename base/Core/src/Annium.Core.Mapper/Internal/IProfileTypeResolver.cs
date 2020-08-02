using System;
using System.Collections.Generic;

namespace Annium.Core.Mapper.Internal
{
    public interface IProfileTypeResolver
    {
        IReadOnlyCollection<Type> ResolveType(Type type);
    }
}