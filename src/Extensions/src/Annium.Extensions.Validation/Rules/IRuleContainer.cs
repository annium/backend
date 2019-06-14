using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public interface IRuleContainer<T>
    {
        BooleanResult Validate(T value, string label = null);
    }
}