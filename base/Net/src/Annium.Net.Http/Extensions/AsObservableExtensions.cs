using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Net.Http
{
    public static class AsObservableExtensions
    {
        public static IObservable<string> AsStringObservable(this IRequest request) =>
            request.ToObservable(Parse.String);

        public static IObservable<ReadOnlyMemory<byte>> AsMemoryObservable(this IRequest request) =>
            request.ToObservable(Parse.Memory);

        public static IObservable<Stream> AsStreamObservable(this IRequest request) =>
            request.ToObservable(Parse.Stream);

        public static IObservable<IResult<T>> AsResultObservable<T>(this IRequest request) =>
            request.ToObservable(Parse.ResultT<T>);

        public static IObservable<T> AsObservable<T>(this IRequest request) =>
            request.ToObservable(Parse.T<T>);

        private static IObservable<T> ToObservable<T>(this IRequest request, Func<HttpContent, Task<T>> parseAsync) => Observable.FromAsync(async () =>
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            var response = await request.RunAsync();

            return await parseAsync(response.Content);
        });
    }
}