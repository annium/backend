using System;
using System.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal class Mapper : IMapper
    {
        private readonly IMapBuilder _mapBuilder;

        public Mapper(IMapBuilder mapBuilder)
        {
            _mapBuilder = mapBuilder;
        }

        public bool HasMap<T>(object source) => HasMap(source, typeof(T));

        public bool HasMap(object source, Type type)
        {
            if (source is null || type is null)
                return false;

            if (type.IsInstanceOfType(source))
                return true;

            return _mapBuilder.HasMap(source.GetType(), type);
        }

        public T Map<T>(object source)
        {
            if (source is null)
                return default!;

            return (T) Map(source, typeof(T));
        }

        public object Map(object source, Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (source is null)
                return Activator.CreateInstance(type)!;

            if (type.IsInstanceOfType(source))
                return source;

            var map = _mapBuilder.GetMap(source.GetType(), type);

            try
            {
                return map.DynamicInvoke(source)!;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        }
    }
}