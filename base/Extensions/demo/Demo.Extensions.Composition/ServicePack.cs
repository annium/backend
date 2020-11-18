using System;
using Annium.Core.DependencyInjection;
using Annium.Core.DependencyInjection.Obsolete;
using Annium.Extensions.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Composition
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddLocalization(opts => opts.UseInMemoryStorage());
            services.AddComposition();
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

    internal class EmailComposer : Composer<IEmail>
    {
        public EmailComposer()
        {
            Field(p => p.Email).LoadWith(ctx => ctx.Label);
        }
    }

    internal class LoginComposer : Composer<ILogin>
    {
        public LoginComposer()
        {
            Field(p => p.Login).LoadWith(ctx => ctx.Label);
        }
    }
}