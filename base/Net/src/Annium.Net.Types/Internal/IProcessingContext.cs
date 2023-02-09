using System;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal interface IProcessingContext
{
    void Process(ContextualType type, Nullability nullability);
    void Process(ContextualType type);
    ModelRef GetRef(ContextualType type);
    void Register(Type typeType, TypeModelBase model);
}