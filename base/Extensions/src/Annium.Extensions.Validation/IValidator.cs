using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public interface IValidator<TValue>
    {
        Task<BooleanResult> ValidateAsync(TValue value, string label = null);
    }
}