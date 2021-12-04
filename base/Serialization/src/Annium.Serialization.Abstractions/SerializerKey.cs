namespace Annium.Serialization.Abstractions;

public record SerializerKey
{
    public static SerializerKey Create(string key, string type) => new(key, type);
    public static SerializerKey CreateDefault(string type) => new(Constants.DefaultKey, type);
    public string Key { get; }
    public string Type { get; }

    private SerializerKey(string key, string type)
    {
        Key = key;
        Type = type;
    }

    public override string ToString() => $"{Key}:{Type}";
}