using System;
using System.Collections.Generic;
using System.Reflection;

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
            public string Name { get; }
            public Type Type { get; }

            public ParameterItem(string name, Type type)
            {
                Name = name;
                Type = type;
            }
        }
    }
}