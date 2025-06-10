using System;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// Provides registration functionality for all NodaTime BSON serializers
/// </summary>
public static class NodaTimeSerializers
{
    /// <summary>
    /// Registers all NodaTime BSON serializers with the MongoDB driver
    /// </summary>
    public static void Register()
    {
        var types = typeof(NodaTimeSerializers)
            .GetTypeInfo()
            .Assembly.DefinedTypes.Where(t =>
                t is { BaseType: not null, ContainsGenericParameters: false }
                && t.ImplementedInterfaces.Contains(typeof(IBsonSerializer))
            )
            .ToList();

        foreach (var type in types)
        {
            var target = type.BaseType!.GenericTypeArguments[0];
            var serializer = (IBsonSerializer)Activator.CreateInstance(type.AsType())!;

            try
            {
                BsonSerializer.RegisterSerializer(target, serializer);
            }
            catch (BsonSerializationException)
            {
                // this catch block is used, because there's no api to check currently registered serializer
            }
        }
    }
}
