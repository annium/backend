using Annium.Core.DependencyInjection;
using Annium.Extensions.Validation;

namespace Demo.Extensions.Validation;

internal class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddLocalization(opts => opts.UseInMemoryStorage());
        container.AddValidation();
    }
}

internal class User : IEmail, ILogin
{
    public string Email { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
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
    public string Text { get; set; } = string.Empty;
}

internal class AValidator : Validator<A>
{
    public AValidator()
    {
        Field(o => o.Text).Required();
    }
}