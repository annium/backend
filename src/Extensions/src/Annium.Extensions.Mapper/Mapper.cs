using System;

namespace Annium.Extensions.Mapper
{
    public class Mapper : IMapper
    {
        private readonly MapBuilder mapBuilder;

        public Mapper(MapBuilder mapBuilder)
        {
            this.mapBuilder = mapBuilder;
        }

        public T Map<T>(object source)
        {
            if (source.GetType() == typeof(T))
                return (T) source;

            var map = mapBuilder.GetMap(source.GetType(), typeof(T));

            return (T) map.DynamicInvoke(source);
        }

        public object Map(object source, Type type)
        {
            if (source.GetType() == type)
                return source;

            var map = mapBuilder.GetMap(source.GetType(), type);

            return map.DynamicInvoke(source);
        }
    }
}