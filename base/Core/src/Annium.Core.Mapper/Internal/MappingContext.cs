using System;

namespace Annium.Core.Mapper.Internal
{
    public class MappingContext : IMappingContext
    {
        private readonly Func<Type, Type, Delegate> _getMap;
        private readonly Func<Type, Type, Mapping> _resolveMapping;

        public MappingContext(
            Func<Type, Type, Delegate> getMap,
            Func<Type, Type, Mapping> resolveMapping
        )
        {
            _getMap = getMap;
            _resolveMapping = resolveMapping;
        }

        public Delegate GetMap(Type src, Type tgt) => _getMap(src, tgt);

        public Mapping ResolveMapping(Type src, Type tgt) => _resolveMapping(src, tgt);
    }
}