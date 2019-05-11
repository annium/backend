using System;
using Annium.Extensions.DependencyInjection;
using Annium.Extensions.Mapper;
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
}