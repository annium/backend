using System;

namespace Annium.Extensions.Mapper
{
    public interface IMapper
    {
        T Map<T>(object source);

        object Map(object source, Type type);
    }
}