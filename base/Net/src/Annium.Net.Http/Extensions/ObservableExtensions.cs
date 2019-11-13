using System;
using System.IO;
using System.Net.Mime;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class ObservableExtensions
    {
        public static IObservable<string> AsStringObservable(this IRequest request) => Observable.FromAsync(async () =>
        {
            var response = await request.RunAsync();

            return await response.Content.ReadAsStringAsync();
        });

        public static IObservable<ReadOnlyMemory<byte>> AsByteArrayObservable(this IRequest request) => Observable.FromAsync(async () =>
        {
            var response = await request.RunAsync();

            var data = await response.Content.ReadAsByteArrayAsync();

            return new ReadOnlyMemory<byte>(data);
        });

        public static IObservable<Stream> AsStreamObservable(this IRequest request) => Observable.FromAsync(async () =>
        {
            var response = await request.RunAsync();

            return await response.Content.ReadAsStreamAsync();
        });

        public static IObservable<IResult<T>> AsResultObservable<T>(this IRequest request) => request.AsObservable<IResult<T>>();

        public static IObservable<T> AsObservable<T>(this IRequest request) => Observable.FromAsync(async () =>
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            var response = await request.RunAsync();

            return await response.Content.ParseAsync<T>();
        });
    }
}