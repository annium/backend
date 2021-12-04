using System;

namespace Annium.Core.DependencyInjection;

public interface IAsyncServiceScope : IAsyncDisposable
{
    IServiceProvider ServiceProvider { get; }
}