using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class ResponseObservableExtensions
    {
        public static IObservable<IResponse<string>> AsResponseStringObservable(this IRequest request) =>
            request.ToResponseObservable(Parse.String);

        public static IObservable<IResponse<ReadOnlyMemory<byte>>> AsResponseMemoryObservable(this IRequest request) =>
            request.ToResponseObservable(Parse.Memory);

        public static IObservable<IResponse<Stream>> AsResponseStreamObservable(this IRequest request) =>
            request.ToResponseObservable(Parse.Stream);

        public static IObservable<IResponse<IResult<T>>> AsResponseResultObservable<T>(this IRequest request) =>
            request.ToResponseObservable(Parse.ResultT<T>);

        public static IObservable<IResponse<T>> AsResponseObservable<T>(this IRequest request) =>
            request.ToResponseObservable(Parse.T<T>);

        private static IObservable<IResponse<T>> ToResponseObservable<T>(
            this IRequest request,
            Func<HttpContent, Task<T>> parseAsync
        ) => Observable.FromAsync(async () =>
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            var response = await request.RunAsync();
            var data = await parseAsync(response.Content);

            return new Response<T>(response, data);
        });

        public static IObservable<IResponse> AsObservable(this IRequest request) => Observable.FromAsync(() =>
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            return request.RunAsync();
        });
    }
}