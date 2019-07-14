namespace Annium.Storage.Abstractions
{
    public interface IStorageFactory
    {
        StorageBase CreateStorage(ConfigurationBase configuration);
    }
}