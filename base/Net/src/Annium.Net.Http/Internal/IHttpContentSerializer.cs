namespace Annium.Net.Http.Internal;

internal interface IHttpContentSerializer
{
    bool CanSerialize(string mediaType);
    string Serialize<T>(string mediaType, T value);
    T Deserialize<T>(string mediaType, string value);
}