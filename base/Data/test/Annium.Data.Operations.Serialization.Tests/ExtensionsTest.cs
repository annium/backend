using System;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Newtonsoft.Json;

namespace Annium.Data.Operations.Serialization.Tests
{
    public class ExtensionsTest
    {
        [Fact]
        public void Configure_NullSettings_Throws()
        {
            // arrange
            Action configure = (() => (null as JsonSerializerSettings).ConfigureForOperations());

            // assert
            configure.Throws<ArgumentNullException>();
        }

        [Fact]
        public void Configure_ExistentSettings_ConfiguresCorrectly()
        {
            // arrange
            var serializer = new JsonSerializerSettings().ConfigureForOperations();

            // assert
            serializer.Converters.OfType<BooleanResultConverter>().Has(1);
            serializer.Converters.OfType<StatusResultConverter>().Has(1);
        }
    }
}