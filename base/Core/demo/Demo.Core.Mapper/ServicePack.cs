using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;

namespace Demo.Core.Mapper;

internal class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddProfile(ConfigureProfile);
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddMapper();
    }

    private void ConfigureProfile(Profile p)
    {
        p.Map<Plain, Complex>()
            .For(x => x.Client!, x => new Client { Name = x.ClientName! });
        p.Map<Complex, Plain>()
            .For(x => x.ClientName!, x => x.Client!.Name!);
        p.Map<A, B>()
            .For(x => x.LowerText, x => x.Text!.ToLower())
            .Ignore(x => x.Ignored);
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

internal class Plain
{
    public string? ClientName { get; set; }
}

internal class Complex
{
    public Client? Client { get; set; }
}

internal class Client
{
    public string? Name { get; set; }
}