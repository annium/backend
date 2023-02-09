using System.Collections.Generic;
using Annium.Net.Types.Internal.Mappers;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal class ModelMapper : IModelMapper
{
    private readonly MapperContext _ctx;

    public ModelMapper()
    {
        _ctx = new MapperContext(this);
    }

    public ITypeModel Map(ContextualType type)
    {
        _ctx.Init(type);
        var model = CoreMapper.Map(type, _ctx);
        _ctx.Register(type, model);

        return model;
    }

    public IReadOnlyCollection<ITypeModel> GetModels() => _ctx.GetModels();
}