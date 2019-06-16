using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public interface IRuleContainer<T>
    {
        Task<BooleanResult> Validate(T value, ValidationContext<T> context);
    }
}