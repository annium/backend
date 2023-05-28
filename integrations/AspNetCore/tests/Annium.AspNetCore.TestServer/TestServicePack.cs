using System;
using Annium.Core.DependencyInjection;

namespace Annium.AspNetCore.TestServer;

public class TestServicePack : ServicePackBase
{
    public TestServicePack()
    {
        Console.WriteLine(nameof(TestServicePack));
        Add<BaseServicePack>();
    }
}