using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper.Internal;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper
{
    public static class Mapper
    {
        private static readonly object locker = new object();

        private static readonly IList<Profile> profiles = new List<Profile>();

        private static IMapper mapper = InitMapper();

        public static void AddConfiguration(Action<Profile> configure)
        {
            var cfg = new EmptyProfile();
            configure(cfg);
            mapper = AddProfile(cfg);
        }

        public static bool HasMap<T>(object source) => HasMap(source, typeof(T));

        public static bool HasMap(object source, Type type) => mapper.HasMap(source, type);

        public static T Map<T>(object source) => mapper.Map<T>(source);

        public static object Map(object source, Type type) => mapper.Map(source, type);

        private static IMapper InitMapper()
        {
            return AddProfile(new DefaultProfile());
        }

        private static IMapper AddProfile(Profile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            lock (locker)
            {
                profiles.Add(profile);
                return CreateMapper();
            }
        }

        private static IMapper CreateMapper()
        {
            var builder = new MapBuilder(
                profiles.ToArray(),
                TypeManager.Instance,
                new Repacker()
            );

            return new MapperInstance(builder);
        }
    }
}