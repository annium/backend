using System;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Annium.MongoDb.NodaTime
{
    public static class NodaTimeSerializers
    {
        public static void Register()
        {
            var types = typeof(NodaTimeSerializers).GetTypeInfo().Assembly.DefinedTypes
                .Where(t => t.BaseType != null && !t.ContainsGenericParameters && t.ImplementedInterfaces.Contains(typeof(IBsonSerializer)))
                .ToList();

            foreach (var type in types)
            {
                var target = type.BaseType.GenericTypeArguments[0];
                var serializer = (IBsonSerializer) Activator.CreateInstance(type.AsType());

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
}