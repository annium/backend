using System;
using System.Reflection;

namespace Annium.Extensions.Mapper
{
    internal class Mapper : IMapper
    {
        private readonly MapBuilder mapBuilder;

        public Mapper(MapBuilder mapBuilder)
        {
            this.mapBuilder = mapBuilder;
        }

        public T Map<T>(object source)
        {
            if (source == null)
                return default(T);

            if (source.GetType() == typeof(T))
                return (T) source;

            var map = mapBuilder.GetMap(source.GetType(), typeof(T));

            try
            {
                return (T) map.DynamicInvoke(source);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public object Map(object source, Type type)
        {
            if (source.GetType() == type)
                return source;

            var map = mapBuilder.GetMap(source.GetType(), type);

            try
            {
                return map.DynamicInvoke(source);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}