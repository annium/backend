using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Extensions.Mapper
{
    public class Mapper : IMapper
    {
        private static object locker = new object();

        private static IList<MapperConfiguration> configurations = new List<MapperConfiguration>();

        private static IMapper mapper;

        static Mapper()
        {
            mapper = CreateMapper();
        }

        public static void AddConfiguration(MapperConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            lock(locker)
            {
                configurations.Add(configuration);
                mapper = CreateMapper();
            }
        }

        private static IMapper CreateMapper()
        {
            var builder = new MapBuilder(
                MapperConfiguration.Merge(configurations.ToArray()),
                TypeResolver.Instance.Value,
                new Repacker()
            );

            return new Mapper(builder);
        }

        private readonly MapBuilder mapBuilder;

        internal Mapper(MapBuilder mapBuilder)
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