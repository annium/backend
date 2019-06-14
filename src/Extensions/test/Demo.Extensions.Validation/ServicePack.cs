using Annium.Extensions.DependencyInjection;
using Annium.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Validation
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddValidation();
        }
    }

    internal class A
    {
        public string Text { get; set; }
    }

    internal class AValidator : Validator<A>
    {
        public AValidator()
        {
            Field(o => o.Text).IsRequired();
        }
    }
}