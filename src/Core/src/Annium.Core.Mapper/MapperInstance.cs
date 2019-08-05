using System;
using System.Reflection;

namespace Annium.Extensions.Mapper
{
    public class MapperInstance : IMapper
    {
        private readonly MapBuilder mapBuilder;

        internal MapperInstance(MapBuilder mapBuilder)
        {
            this.mapBuilder = mapBuilder;
        }

        public bool HasMap<T>(object source) => HasMap(source, typeof(T));

        public bool HasMap(object source, Type type)
        {
            if (source == null || type == null)
                return false;

            return mapBuilder.HasMap(source.GetType(), type);
        }

        public T Map<T>(object source)
        {
            if (source == null)
                return default(T);

            return (T) Map(source, typeof(T));
        }

        public object Map(object source, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (source == null)
                return Activator.CreateInstance(type);

            if (source.GetType() == type)
                return source;

            if (type.IsEnum)
                return Enum.Parse(type, source.ToString(), ignoreCase : true);

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