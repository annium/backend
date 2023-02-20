using System;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Config;

internal interface IMapperConfigInternal : IMapperConfig
{
    BaseTypeRef? GetBaseTypeRefFor(Type type);
    bool IsIgnored(ContextualType type);
    bool IsExcluded(ContextualType type);
    bool IsArray(ContextualType type);
    bool IsRecord(ContextualType type);
}