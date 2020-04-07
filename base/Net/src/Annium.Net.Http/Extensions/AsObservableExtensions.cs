using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class AsObservableExtensions
    {
        public static IObservable<string> AsStringObservable(this IRequest request) =>
            request.ToObservable(Parse.String);

        public static IObservable<string> AsStringObservable(this IRequest request, string defaultValue) =>
            request.ToObservable(Parse.String, defaultValue);

        public static IObservable<ReadOnlyMemory<byte>> AsMemoryObservable(this IRequest request) =>
            request.ToObservable(Parse.Memory);

        public static IObservable<ReadOnlyMemory<byte>> AsMemoryObservable(this IRequest request, ReadOnlyMemory<byte> defaultValue) =>
            request.ToObservable(Parse.Memory, defaultValue);

        public static IObservable<Stream> AsStreamObservable(this IRequest request) =>
            request.ToObservable(Parse.Stream);

        public static IObservable<Stream> AsStreamObservable(this IRequest request, Stream defaultValue) =>
            request.ToObservable(Parse.Stream, defaultValue);

        public static IObservable<IResult<T>> AsResultObservable<T>(this IRequest request) =>
            request.ToObservable(Parse.ResultT<T>);

        public static IObservable<IResult<T>> AsResultObservable<T>(this IRequest request, IResult<T> defaultValue) =>
            request.ToObservable(Parse.ResultT<T>, defaultValue);

        public static IObservable<T> AsObservable<T>(this IRequest request) =>
            request.ToObservable(Parse.T<T>);

        public static IObservable<T> AsObservable<T>(this IRequest request, T defaultValue) =>
            request.ToObservable(Parse.T<T>, defaultValue);

        private static IObservable<T> ToObservable<T>(this IRequest request, Func<HttpContent, Task<T>> parseAsync) =>
            Observable.FromAsync(async () =>
            {
                if (!request.IsEnsuringSuccess)
                    request.EnsureSuccessStatusCode();

                var response = await request.RunAsync();

                return await parseAsync(response.Content);
            });

        private static IObservable<T> ToObservable<T>(this IRequest request, Func<HttpContent, T, Task<T>> parseAsync, T defaultValue) =>
            Observable.FromAsync(async () =>
            {
                try
                {
                    if (!request.IsEnsuringSuccess)
                        request.EnsureSuccessStatusCode();

                    var response = await request.RunAsync();

                    return await parseAsync(response.Content, defaultValue);
                }
                catch
                {
                    return defaultValue;
                }
            });
    }
}