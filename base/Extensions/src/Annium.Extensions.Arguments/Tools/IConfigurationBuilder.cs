namespace Annium.Extensions.Arguments
{
    internal interface IConfigurationBuilder
    {
        T Build<T>(string[] args)
            where T : new();
    }
}