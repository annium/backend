namespace Annium.Net.Http
{
    public interface IHttpContentSerializer
    {
        bool CanSerialize(string mediaType);
        string Serialize<T>(string mediaType, T value);
        T Deserialize<T>(string mediaType, string value);
    }
}