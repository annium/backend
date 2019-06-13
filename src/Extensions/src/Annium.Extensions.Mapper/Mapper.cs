using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Application.Types;

namespace Annium.Extensions.Mapper
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

        public static bool HasMap<T>(object source) => HasMap(source, typeof(T));

        public static bool HasMap(object source, Type type) => mapper.HasMap(source, type);

        public static T Map<T>(object source) => mapper.Map<T>(source);

        public static object Map(object source, Type type) => mapper.Map(source, type);

        private static IMapper CreateMapper()
        {
            var builder = new MapBuilder(
                MapperConfiguration.Merge(configurations.ToArray()),
                TypeManager.Instance,
                new Repacker()
            );

            return new MapperInstance(builder);
        }
    }
}