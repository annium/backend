using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Microsoft.Extensions.DependencyInjection;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Base.Mapper;

public interface ITestProvider
{
    IReadOnlyCollection<IModel> Models { get; }
    void ConfigureContainer(ServiceContainer container);
    void Setup(ServiceProvider sp);
    IRef Map(ContextualType type);
}