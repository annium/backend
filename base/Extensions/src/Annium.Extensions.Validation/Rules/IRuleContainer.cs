using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    public interface IRuleContainer<T>
    {
        int StageCount { get; }

        Task Validate(ValidationContext<T> context, T value, int stage);
    }
}