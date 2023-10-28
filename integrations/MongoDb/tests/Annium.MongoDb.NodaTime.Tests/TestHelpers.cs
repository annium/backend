using MongoDB.Bson;

namespace Annium.MongoDb.NodaTime.Tests;

internal static class TestHelpers
{
    public static string ToTestJson<TType>(this TType obj)
    {
        return obj.ToJson().Replace('"', '\'');
    }
}
