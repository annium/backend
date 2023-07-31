using System;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

public class GenericProfilesTest
{
    [Fact]
    public void GenericProfiles_Work()
    {
        // arrange
        var mapper = GetMapper(typeof(ValidProfile<>));
        var b = new B { Name = "Mike", Age = 5 };
        var c = new C { Name = "Donny", IsAlive = true };

        // act
        var one = mapper.Map<D>(b);
        var two = mapper.Map<D>(c);

        // assert
        one.LowerName.Is("mike");
        two.LowerName.Is("donny");
    }

    [Fact]
    public void GenericProfiles_Unconstrained_Fails()
    {
        // arrange
        Wrap.It(() => GetMapper(typeof(InvalidProfile<>))).Throws<ArgumentException>();
    }

    private IMapper GetMapper(Type profileType) => new ServiceContainer()
        .AddRuntime(Assembly.GetCallingAssembly())
        .AddMapper(autoload: false)
        .AddProfile(profileType)
        .BuildServiceProvider()
        .Resolve<IMapper>();

    private class ValidProfile<T> : Profile
        where T : A
    {
        public ValidProfile()
        {
            Map<T, D>(x => new D { LowerName = x.Name.ToLowerInvariant() });
        }
    }

    private class InvalidProfile<T> : Profile
    {
        public InvalidProfile()
        {
            Map<T, D>(x => new D());
        }
    }

    private class A
    {
        public string Name { get; set; } = string.Empty;
    }

    [AutoMapped]
    private class B : A
    {
        public int Age { get; set; }
    }

    [AutoMapped]
    private class C : A
    {
        public bool IsAlive { get; set; }
    }

    private class D
    {
        public string LowerName { get; set; } = string.Empty;
    }
}