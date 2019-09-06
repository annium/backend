using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;
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

    internal class User : IEmail, ILogin
    {
        public string Email { get; set; }
        public string Login { get; set; }
    }

    internal interface IEmail
    {
        string Email { get; set; }
    }

    internal interface ILogin
    {
        string Login { get; set; }
    }

    internal class EmailValidator : Validator<IEmail>
    {
        public EmailValidator()
        {
            Field(p => p.Email).Required();
        }
    }

    internal class LoginValidator : Validator<ILogin>
    {
        public LoginValidator()
        {
            Field(p => p.Login).Required();
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
            Field(o => o.Text).Required();
        }
    }
}