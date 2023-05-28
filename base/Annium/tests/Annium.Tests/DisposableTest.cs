using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class DisposableTest
{
    [Fact]
    public async Task AsyncDisposable_Add_Works()
    {
        // arrange
        var box = Disposable.AsyncBox();
        var calls = 0;

        // act
        box += Disposable.Create(() => ++calls);
        box += Disposable.Create(() => Task.FromResult(++calls));
        box += () => ++calls;
        box += () => Task.FromResult(++calls);
        await box.DisposeAsync();

        // assert
        calls.Is(4);
    }

    [Fact]
    public async Task AsyncDisposable_Remove_Works()
    {
        // arrange
        var box = Disposable.AsyncBox();
        var calls = 0;

        // act
        var disposable = Disposable.Create(() => ++calls);
        var asyncDisposable = Disposable.Create(() => Task.FromResult(++calls));
        void Dispose() => ++calls;
        Task AsyncDispose() => Task.FromResult(++calls);
        box += disposable;
        box -= disposable;
        box += asyncDisposable;
        box -= asyncDisposable;
        box += Dispose;
        box -= Dispose;
        box += AsyncDispose;
        box -= AsyncDispose;
        await box.DisposeAsync();

        // assert
        calls.Is(0);
    }

    [Fact]
    public async Task AsyncDisposable_Reset_Works()
    {
        // arrange
        var box = Disposable.AsyncBox();

        // act
        await box.DisposeAndResetAsync();

        // assert
        box.IsDisposed.IsFalse();
    }

    [Fact]
    public void Disposable_Add_Works()
    {
        // arrange
        var box = Disposable.Box();
        var calls = 0;

        // act
        box += Disposable.Create(() => ++calls);
        box += () => ++calls;
        box.Dispose();

        // assert
        calls.Is(2);
    }

    [Fact]
    public void Disposable_Remove_Works()
    {
        // arrange
        var box = Disposable.Box();
        var calls = 0;

        // act
        var disposable = Disposable.Create(() => ++calls);
        void Dispose() => ++calls;
        box += disposable;
        box -= disposable;
        box += Dispose;
        box -= Dispose;
        box.Dispose();

        // assert
        calls.Is(0);
    }

    [Fact]
    public void Disposable_Reset_Works()
    {
        // arrange
        var box = Disposable.Box();

        // act
        box.DisposeAndReset();

        // assert
        box.IsDisposed.IsFalse();
    }
}