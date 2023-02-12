using System;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal interface IProcessingContext
{
    void Process(ContextualType type, Nullability nullability);
    void Process(ContextualType type);
    IRef GetRef(ContextualType type, Nullability nullability);
    IRef GetRef(ContextualType type);
    IRef RequireRef(ContextualType type);
    bool IsRegistered(Type type);
    void Register(Type type, ModelBase model);
}