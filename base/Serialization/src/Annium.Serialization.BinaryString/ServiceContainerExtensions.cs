using Annium.Serialization.Abstractions;
using Annium.Serialization.BinaryString.Internal;
using Constants = Annium.Serialization.BinaryString.Constants;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddBinaryStringSerializer(this IServiceContainer container) =>
            container.AddBinaryStringSerializerInternal(Annium.Serialization.Abstractions.Constants.DefaultKey);

        public static IServiceContainer AddBinaryStringSerializer(this IServiceContainer container, string key) =>
            container.AddBinaryStringSerializerInternal(key);

        private static IServiceContainer AddBinaryStringSerializerInternal(this IServiceContainer container, string key)
        {
            var fullKey = SerializerKey.Create(key, Constants.MediaType);
            container.Add<HexStringSerializer>().AsKeyed<ISerializer<byte[], string>, SerializerKey>(fullKey).Singleton();

            return container;
        }
    }
}