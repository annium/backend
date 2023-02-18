using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class AsExtensions
{
    public static Task<string> AsStringAsync(this IHttpRequest request) =>
        request.ToAsync(Parse.String);

    public static Task<string> AsStringAsync(this IHttpRequest request, string defaultValue) =>
        request.ToAsync(Parse.String, defaultValue);

    public static Task<ReadOnlyMemory<byte>> AsMemoryAsync(this IHttpRequest request) =>
        request.ToAsync(Parse.Memory);

    public static Task<ReadOnlyMemory<byte>> AsMemoryAsync(this IHttpRequest request, ReadOnlyMemory<byte> defaultValue) =>
        request.ToAsync(Parse.Memory, defaultValue);

    public static Task<Stream> AsStreamAsync(this IHttpRequest request) =>
        request.ToAsync(Parse.Stream);

    public static Task<Stream> AsStreamAsync(this IHttpRequest request, Stream defaultValue) =>
        request.ToAsync(Parse.Stream, defaultValue);

    public static Task<IResult> AsResultAsync(this IHttpRequest request) =>
        request.ToAsync(Parse.Result);

    public static Task<IResult> AsResultAsync(this IHttpRequest request, IResult defaultValue) =>
        request.ToAsync(Parse.Result, defaultValue);

    public static Task<IResult<T>> AsResultAsync<T>(this IHttpRequest request) =>
        request.ToAsync(Parse.ResultT<T>);

    public static Task<IResult<T>> AsResultAsync<T>(this IHttpRequest request, IResult<T> defaultValue) =>
        request.ToAsync(Parse.ResultT, defaultValue);

    public static Task<T> AsAsync<T>(this IHttpRequest request) =>
        request.ToAsync(Parse.T<T>);

    public static Task<T> AsAsync<T>(this IHttpRequest request, T defaultValue) =>
        request.ToAsync(Parse.T, defaultValue);

    private static async Task<T> ToAsync<T>(
        this IHttpRequest request,
        Func<IHttpRequest, HttpContent, Task<T>> parseAsync
    )
    {
        if (!request.IsEnsuringSuccess)
            request.EnsureSuccessStatusCode();

        var response = await request.RunAsync();

        return await parseAsync(request, response.Content);
    }

    private static async Task<T> ToAsync<T>(
        this IHttpRequest request,
        Func<IHttpRequest, HttpContent, T, Task<T>> parseAsync,
        T defaultValue
    )
    {
        try
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            var response = await request.RunAsync();

            return await parseAsync(request, response.Content, defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }
}