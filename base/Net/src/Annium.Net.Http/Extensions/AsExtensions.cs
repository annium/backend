using System.Threading.Tasks;
using Annium.Net.Http.Internal;
using OneOf;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class AsExtensions
{
    public static async Task<T?> AsAsync<T>(
        this IHttpRequest request
    )
    {
        var response = await request.RunAsync();

        try
        {
            return await ContentParser.ParseAsync<T>(request.Serializer, response.Content);
        }
        catch
        {
            return default;
        }
    }

    public static async Task<T> AsAsync<T>(
        this IHttpRequest request,
        T defaultData
    )
    {
        var response = await request.RunAsync();

        try
        {
            var data = await ContentParser.ParseAsync<T>(request.Serializer, response.Content);
            return data ?? defaultData;
        }
        catch
        {
            return defaultData;
        }
    }

    public static async Task<OneOf<TSuccess?, TFailure?>> AsAsync<TSuccess, TFailure>(
        this IHttpRequest request
    )
    {
        var response = await request.RunAsync();

        try
        {
            var success = await ContentParser.ParseAsync<TSuccess>(request.Serializer, response.Content);
            if (!Equals(success, default))
                return success;

            var failure = await ContentParser.ParseAsync<TFailure>(request.Serializer, response.Content);
            if (!Equals(failure, default))
                return failure;

            return default(TSuccess)!;
        }
        catch
        {
            return default(TSuccess)!;
        }
    }

    public static async Task<OneOf<TSuccess, TFailure?>> AsAsync<TSuccess, TFailure>(
        this IHttpRequest request,
        TSuccess defaultData
    )
    {
        var response = await request.RunAsync();

        try
        {
            var success = await ContentParser.ParseAsync<TSuccess>(request.Serializer, response.Content);
            if (!Equals(success, default))
                return success;

            var failure = await ContentParser.ParseAsync<TFailure>(request.Serializer, response.Content);
            if (!Equals(failure, default))
                return failure;

            return defaultData;
        }
        catch
        {
            return defaultData;
        }
    }
}