using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Mapper.Internal
{
    internal class MapBuilder : IMapBuilder
    {
        private readonly IReadOnlyCollection<Profile> _knownProfiles;
        private readonly ITypeResolver _typeResolver;
        private readonly IEnumerable<IMapResolver> _mapResolvers;
        private readonly IRepacker _repacker;
        private readonly Lazy<IMapContext> _mapContext;
        private readonly IDictionary<ValueTuple<Type, Type>, Entry> _entries = new Dictionary<ValueTuple<Type, Type>, Entry>();
        private readonly IMapResolverContext _context;

        public MapBuilder(
            IEnumerable<Profile> profiles,
            ITypeResolver typeResolver,
            IEnumerable<IMapResolver> mapResolvers,
            IRepacker repacker,
            Lazy<IMapContext> mapContext
        )
        {
            _knownProfiles = profiles.ToArray();
            _typeResolver = typeResolver;
            _mapResolvers = mapResolvers;
            _repacker = repacker;
            _mapContext = mapContext;
            _context = new MapResolverContext(GetMap, ResolveMapping, mapContext);

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
            lock (entry.MapLock)
            {
                if (entry.HasMap)
                    return entry.Map;

                var param = Expression.Parameter(src);
                var mapping = ResolveMapping(src, tgt);

                var result = Expression.Lambda(mapping(param), param);
                var str = result.ToReadableString();

                entry.SetMap(result.Compile());
            }

            return entry.Map;
        }

        private Mapping ResolveMapping(Type src, Type tgt)
        {
            if (src == tgt)
                return ex => ex;

            var entry = GetEntry((src, tgt));
            lock (entry.MappingLock)
            {
                if (entry.HasMapping)
                    return entry.Mapping;

                entry.SetMapping(() => BuildMapping(src, tgt, entry.HasConfiguration ? entry.Configuration : MapConfiguration.Empty));
            }

            return entry.Mapping;
        }

        private Mapping BuildMapping(Type src, Type tgt, IMapConfiguration cfg)
        {
            var mapResolver = _mapResolvers.OrderBy(x => x.Order).FirstOrDefault(x => x.CanResolveMap(src, tgt));
            if (mapResolver is not null)
                return mapResolver.ResolveMap(src, tgt, cfg, _context);

            throw new MappingException(src, tgt, "No map found.");
        }

        private IMapBuilder AddProfileInternal(
            Type profileType
        )
        {
            var types = _typeResolver.ResolveType(profileType);

            foreach (var type in types)
            {
                var profile = _knownProfiles.SingleOrDefault(x => x.GetType() == type)
                    ?? (Profile) Activator.CreateInstance(type)!;

                AddEntriesFromProfile(profile);
            }

            return this;
        }

        private void AddEntriesFromProfile(Profile profile)
        {
            foreach (var (key, cfg) in profile.MapConfigurations)
            {
                var entry = GetEntry(key);
                if (!entry.HasConfiguration)
                    entry.SetConfiguration(cfg);
                if (!entry.HasMapping && cfg.MapWith is not null)
                    entry.SetMapping(() => _repacker.Repack(cfg.MapWith(_mapContext.Value).Body));
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
            public static Entry Create() => new();

            public bool HasConfiguration => Configuration is not null!;
            public IMapConfiguration Configuration { get; private set; } = default!;
            public object MappingLock = new();
            public bool HasMapping => _mapping is not null!;
            public Mapping Mapping => _mapping.Value;
            public object MapLock = new();
            public bool HasMap => Map is not null!;
            public Delegate Map { get; private set; } = default!;
            private Lazy<Mapping> _mapping = default!;

            private Entry()
            {
            }

            public void SetConfiguration(IMapConfiguration configuration)
            {
                if (HasConfiguration)
                    throw new InvalidOperationException("Configuration already set");

                Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }

            public void SetMapping(Func<Mapping> mapping)
            {
                if (HasMapping)
                    throw new InvalidOperationException("Mapping already set");

                _mapping = new Lazy<Mapping>(mapping, true);
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