using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public interface IValidator<TValue>
    {
        Task<IBooleanResult> ValidateAsync(TValue value, string label = null);
    }
}