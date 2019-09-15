using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper.Internal;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper
{
    public static class Mapper
    {
        private static object locker = new object();

        private static IList<MapperConfiguration> configurations = new List<MapperConfiguration>();

        private static IMapper mapper;

        static Mapper()
        {
            var cfg = new MapperConfiguration();
            DefaultConfiguration.Apply(cfg);
            AddConfiguration(cfg);
        }

        public static void AddConfiguration(Action<MapperConfiguration> configure)
        {
            var cfg = new MapperConfiguration();
            configure(cfg);
            AddConfiguration(cfg);
        }

        public static bool HasMap<T>(object source) => HasMap(source, typeof(T));

        public static bool HasMap(object source, Type type) => mapper.HasMap(source, type);

        public static T Map<T>(object source) => mapper.Map<T>(source);

        public static object Map(object source, Type type) => mapper.Map(source, type);

        private static void AddConfiguration(MapperConfiguration configuration)
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
                configurations.ToArray(),
                TypeManager.Instance,
                new Repacker()
            );

            return new MapperInstance(builder);
        }
    }
}