using System;
using System.Collections.Generic;
using System.Reflection;
using Annium.Core.Primitives;

namespace Annium.Serialization.Json.Internal.Converters
{
    internal class ConstructorJsonConverterConfiguration
    {
        public ConstructorInfo Constructor { get; }
        public List<ParameterItem> Parameters { get; }

        public ConstructorJsonConverterConfiguration(
            ConstructorInfo constructor,
            List<ParameterItem> parameters
        )
        {
            Constructor = constructor;
            Parameters = parameters;
        }

        public void Deconstruct(
            out ConstructorInfo constructor,
            out List<ParameterItem> parameters
        )
        {
            constructor = Constructor;
            parameters = Parameters;
        }

        internal class ParameterItem
        {
            public Type Type { get; }
            public string Name { get; }

            public ParameterItem(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            public override string ToString() => $"{Type.FriendlyName()} {Name}";
        }
    }
}