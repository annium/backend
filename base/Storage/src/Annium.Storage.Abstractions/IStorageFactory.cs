namespace Annium.Storage.Abstractions;

public interface IStorageFactory
{
    IStorage CreateStorage(ConfigurationBase configuration);
}