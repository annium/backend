using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal class MapBuilder : IMapBuilder
    {
        private readonly IReadOnlyCollection<Profile> _knownProfiles;
        private readonly IProfileTypeResolver _profileTypeResolver;
        private readonly IEnumerable<IMapResolver> _mapResolvers;
        private readonly IRepacker _repacker;
        private readonly IDictionary<ValueTuple<Type, Type>, Entry> _entries = new Dictionary<ValueTuple<Type, Type>, Entry>();
        private readonly IMappingContext _context;

        public MapBuilder(
            IEnumerable<Profile> profiles,
            IProfileTypeResolver profileTypeResolver,
            IEnumerable<IMapResolver> mapResolvers,
            IRepacker repacker
        )
        {
            _knownProfiles = profiles.ToArray();
            _profileTypeResolver = profileTypeResolver;
            _mapResolvers = mapResolvers;
            _repacker = repacker;
            _context = new MappingContext(GetMap, ResolveMapping);

            foreach (var profile in _knownProfiles)
                AddEntriesFromProfile(profile);
        }

        public IMapBuilder AddProfile(Action<Profile> configure)
        {
            var profile = new EmptyProfile();
            configure(profile);

            AddEntriesFromProfile(profile);

            return this;
        }

        public IMapBuilder AddProfile<T>()
            where T : Profile
            => AddProfileInternal(typeof(T));

        public IMapBuilder AddProfile(Type profileType)
        {
            if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
                throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

            return AddProfileInternal(profileType);
        }

        public bool HasMap(Type src, Type tgt) => src == tgt || GetEntry((src, tgt)).HasMapping;

        public Delegate GetMap(Type src, Type tgt)
        {
            var entry = GetEntry((src, tgt));
            if (entry.HasMap)
                return entry.Map;

            var param = Expression.Parameter(src);
            var mapping = ResolveMapping(src, tgt);

            var result = Expression.Lambda(mapping(param), param);
            var str = result.ToReadableString();

            entry.SetMap(result.Compile());

            return entry.Map;
        }

        private Mapping ResolveMapping(Type src, Type tgt)
        {
            if (src == tgt)
                return ex => ex;

            var entry = GetEntry((src, tgt));
            if (entry.HasMapping)
                return entry.Mapping;

            entry.SetMapping(BuildMapping(src, tgt, entry.HasConfiguration ? entry.Configuration : MapConfiguration.Empty));

            return entry.Mapping;
        }

        private Mapping BuildMapping(Type src, Type tgt, IMapConfiguration cfg)
        {
            var mapResolver = _mapResolvers.OrderBy(x => x.Order).FirstOrDefault(x => x.CanResolveMap(src, tgt));
            if (mapResolver != null)
                return mapResolver.ResolveMap(src, tgt, cfg, _context);

            throw new MappingException(src, tgt, "No map found.");
        }

        private IMapBuilder AddProfileInternal(
            Type profileType
        )
        {
            var types = _profileTypeResolver.ResolveType(profileType);

            foreach (var type in types)
            {
                var profile = _knownProfiles.SingleOrDefault(x => x.GetType() == type)
                    ?? (Profile) Activator.CreateInstance(type);

                AddEntriesFromProfile(profile);
            }

            return this;
        }

        private void AddEntriesFromProfile(Profile profile)
        {
            foreach (var ( key, cfg) in profile.MapConfigurations)
            {
                var entry = GetEntry(key);
                if (!entry.HasConfiguration)
                    entry.SetConfiguration(cfg);
                if (!entry.HasMapping && cfg.MapWith != null)
                    entry.SetMapping(_repacker.Repack(cfg.MapWith.Body));
            }
        }

        private Entry GetEntry((Type, Type) key)
        {
            lock (_entries)
            {
                if (_entries.TryGetValue(key, out var entry))
                    return entry;

                return _entries[key] = Entry.Create();
            }
        }

        private class Entry
        {
            public static Entry Create() => new Entry();

            public bool HasConfiguration => Configuration != null;
            public IMapConfiguration Configuration { get; private set; } = default!;
            public bool HasMapping => Mapping != null;
            public Mapping Mapping { get; private set; } = default!;
            public bool HasMap => Map != null;
            public Delegate Map { get; private set; } = default!;

            private Entry()
            {
            }

            public void SetConfiguration(IMapConfiguration configuration)
            {
                if (HasConfiguration)
                    throw new InvalidOperationException("Configuration already set");

                Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }

            public void SetMapping(Mapping mapping)
            {
                if (HasMapping)
                    throw new InvalidOperationException("Mapping already set");

                Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            }

            public void SetMap(Delegate map)
            {
                if (HasMap)
                    throw new InvalidOperationException("Map already set");

                Map = map ?? throw new ArgumentNullException(nameof(map));
            }
        }
    }
}