using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;
using Annium.Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Validation
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddLocalization(opts => opts.UseInMemoryStorage());
            services.AddValidation();
        }
    }

    internal class A
    {
        public string Text { get; set; }
    }

    internal class AValidator : Validator<A>
    {
        public AValidator(ILocalizer<AValidator> localizer) : base(localizer)
        {
            Field(o => o.Text).Required();
        }
    }
}