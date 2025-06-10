using MongoDB.Bson;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Helper methods for testing serialization functionality
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Converts an object to JSON string for testing purposes, replacing double quotes with single quotes
    /// </summary>
    /// <typeparam name="TType">The type of object to convert</typeparam>
    /// <param name="obj">The object to convert to JSON</param>
    /// <returns>JSON string with single quotes for easier string matching in tests</returns>
    public static string ToTestJson<TType>(this TType obj)
    {
        return obj.ToJson().Replace('"', '\'');
    }
}
