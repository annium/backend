using System.Collections.Generic;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

public interface IGenericModel : IModel
{
    IReadOnlyList<GenericParameterRef> Args { get; }
}