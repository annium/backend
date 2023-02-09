using System.Collections.Generic;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal class ModelMapper : IModelMapper
{
    private readonly ProcessingContext _ctx;

    public ModelMapper()
    {
        _ctx = new ProcessingContext();
    }

    public ModelRef Map(ContextualType type)
    {
        _ctx.Process(type);

        return _ctx.GetRef(type);
    }

    public IReadOnlyCollection<TypeModelBase> GetModels() => _ctx.GetModels();
}