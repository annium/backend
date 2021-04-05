using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests
{
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
            Action dispose = () => ++calls;
            Func<Task> asyncDispose = () => Task.FromResult(++calls);
            box += disposable;
            box -= disposable;
            box += asyncDisposable;
            box -= asyncDisposable;
            box += dispose;
            box -= dispose;
            box += asyncDispose;
            box -= asyncDispose;
            await box.DisposeAsync();

            // assert
            calls.Is(0);
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
            Action dispose = () => ++calls;
            box += disposable;
            box -= disposable;
            box += dispose;
            box -= dispose;
            box.Dispose();

            // assert
            calls.Is(0);
        }
    }
}