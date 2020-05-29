using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mapper
{
    public static class Mapper
    {
        private static readonly object Locker = new object();

        private static readonly IList<Profile> Profiles = new List<Profile>();

        private static IMapper _mapper = InitMapper();

        public static void AddConfiguration(Action<Profile> configure)
        {
            var cfg = new EmptyProfile();
            configure(cfg);
            _mapper = AddProfile(cfg);
        }

        public static bool HasMap<T>(object source) => HasMap(source, typeof(T));

        public static bool HasMap(object source, Type type) => _mapper.HasMap(source, type);

        public static T Map<T>(object source) => _mapper.Map<T>(source);

        public static object Map(object source, Type type) => _mapper.Map(source, type);

        private static IMapper InitMapper()
        {
            return AddProfile(new DefaultProfile());
        }

        private static IMapper AddProfile(Profile profile)
        {
            if (profile is null)
                throw new ArgumentNullException(nameof(profile));

            lock (Locker)
            {
                Profiles.Add(profile);
                return CreateMapper();
            }
        }

        private static IMapper CreateMapper()
        {
            var provider = new ServiceCollection().AddMapper(false).BuildServiceProvider();
            var builder = new MapBuilder(
                Profiles.ToArray(),
                provider.GetRequiredService<IEnumerable<IMapResolver>>(),
                new Repacker()
            );

            return new MapperInstance(builder);
        }
    }
}