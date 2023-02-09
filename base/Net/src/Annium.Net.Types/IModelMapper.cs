using System.Collections.Generic;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types;

public interface IModelMapper
{
    ModelRef Map(ContextualType type);
    IReadOnlyCollection<ITypeModel> GetModels();
}