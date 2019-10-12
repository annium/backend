using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Mapping
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddProfile(ConfigureProfile);
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddMapper();
        }

        private void ConfigureProfile(Profile p)
        {
            p.Map<A, B>(x => new B(0, x.Text!.ToLower()));

            p.Map<Plain, Complex>()
                .Field(d => d.Client).When((s, f) => s != null && f != null).With(x => new Client { Name = x.ClientName })
                .Field(d => d.Client).When((s, f) => s is null || f is null).Throw(_ => new ArgumentException("Source has empty fields"))
                .Field(d => d.Client!.Tags).When(f => f.Length > 0).From(s => s.ClientTags).When(x => x.Text != null).With(x => x.Text!.ToString());

            p.MapGeneric<GenA<object>, GenB<object>>()

            // p.Map<Plain, Complex>()
            //     .Field(e => new Client { Name = e.ClientName! }, e => e.Client!);
            // p.Map<Complex, Plain>()
            //     .Field(e => e.Client!.Name!, e => e.ClientName!);
            // p.Map<A, B>()
            //     .Field(a => a.Text!.ToLower(), b => b.LowerText)
            //     .Ignore(b => b.Ignored);
        }
    }

    internal class A
    {
        public string? Text { get; set; }
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

    internal class GenA<T>
    {
        public string? Text { get; set; }
    }

    internal class GenB<T>
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
        public string? ClientName { get; set; }
        public A[] ClientTags { get; set; } = Array.Empty<A>();
        public Dictionary<string, int> ClientStorage { get; set; } = new Dictionary<string, int>();
    }

    internal class Complex
    {
        public Client? Client { get; set; }

        public uint ManualRank { get; set; }
    }

    internal class Client
    {
        public string? Name { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public Dictionary<string, uint> Storage { get; set; } = new Dictionary<string, uint>();
    }
}
