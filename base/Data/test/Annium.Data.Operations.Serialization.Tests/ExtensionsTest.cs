using System.Linq;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Testing;

namespace Annium.Data.Operations.Serialization.Tests
{
    public class ExtensionsTest
    {
        [Fact]
        public void Configure_Options_AreConfiguredCorrectly()
        {
            // arrange
            var serializer = new JsonSerializerOptions().ConfigureForOperations();

            // assert
            serializer.Converters.OfType<ResultConverter>().Has(1);
            serializer.Converters.OfType<ResultDataConverter>().Has(1);
            serializer.Converters.OfType<StatusResultConverter>().Has(1);
            serializer.Converters.OfType<StatusDataResultConverter>().Has(1);
            serializer.Converters.OfType<BooleanResultConverter>().Has(1);
            serializer.Converters.OfType<BooleanDataResultConverter>().Has(1);
        }
    }
}