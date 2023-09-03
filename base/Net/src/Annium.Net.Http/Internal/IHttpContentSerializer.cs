namespace Annium.Net.Http.Internal;

internal interface IHttpContentSerializer
{
    string Serialize<T>(string mediaType, T value);
    T Deserialize<T>(string mediaType, string value);
}