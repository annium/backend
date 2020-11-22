using Annium.Serialization.Abstractions;
using Annium.Serialization.BinaryString.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddBinaryStringSerializer(this IServiceContainer container)
        {
            container.Add<HexStringSerializer>().AsKeyed<ISerializer<byte[], string>, string>(Constants.Key);

            return container;
        }
    }
}