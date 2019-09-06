using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    internal interface IRuleContainer<TValue>
    {
        int StageCount { get; }

        Task ValidateAsync(ValidationContext<TValue> context, TValue value, int stage);
    }
}