using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    public interface IRuleContainer<T>
    {
        Task Validate(T value, ValidationContext<T> context);
    }
}