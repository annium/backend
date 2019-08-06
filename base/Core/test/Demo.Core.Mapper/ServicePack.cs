using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Mapping
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddMapperConfiguration(ConfigureMapping);
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddMapper(provider);
        }

        private void ConfigureMapping(MapperConfiguration cfg)
        {
            cfg.Map<Plain, Complex>()
                .Field(e => new Client { Name = e.ClientName }, e => e.Client);
            cfg.Map<Complex, Plain>()
                .Field(e => e.Client.Name, e => e.ClientName);
            cfg.Map<A, B>()
                .Field(a => a.Text.ToLower(), b => b.LowerText)
                .Ignore(b => b.Ignored);
        }
    }

    internal class A
    {
        public string Text { get; set; }
    }

    internal class B
    {
        public int Ignored { get; set; }

        public string LowerText { get; set; }

        public B(int ignored, string lowerText)
        {
            Ignored = ignored;
            LowerText = lowerText;
        }
    }

    internal class Plain
    {
        public string ClientName { get; set; }
    }

    internal class Complex
    {
        public Client Client { get; set; }
    }

    internal class Client
    {
        public string Name { get; set; }
    }
}