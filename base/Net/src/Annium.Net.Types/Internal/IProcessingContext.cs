using System;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal interface IProcessingContext
{
    void Process(ContextualType type, Nullability nullability);
    void Process(ContextualType type);
    ModelRef GetRef(ContextualType type, Nullability nullability);
    ModelRef GetRef(ContextualType type);
    ModelRef RequireRef(ContextualType type);
    void Register(Type type, TypeModelBase model);
}