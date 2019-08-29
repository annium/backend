using Annium.Testing;
using Newtonsoft.Json;

namespace Annium.Data.Operations.Serialization.Tests
{
    public class ResultConverterTest
    {
        [Fact]
        public void BaseWrite_Base_WritesCorrectly()
        {
            // act
            var str = JsonConvert.SerializeObject(Result.New(), GetSettings());

            // assert
            str.IsEqual(@"{""plainErrors"":[],""labeledErrors"":{}}");
        }

        [Fact]
        public void BaseWrite_WithErrors_WritesCorrectly()
        {
            // act
            var str = JsonConvert.SerializeObject(Result.New().Error("plain").Error("label", "another"), GetSettings());

            // assert
            str.IsEqual(@"{""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
        }

        [Fact]
        public void BaseRead_Blank_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IResult>("{}", GetSettings());

            // assert
            result.HasErrors.IsFalse();
        }

        [Fact]
        public void BaseRead_WithErrors_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IResult>(@"{""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}", GetSettings());

            // assert
            result.HasErrors.IsTrue();
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("plain");
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At("label").At(0).IsEqual("another");
        }

        private JsonSerializerSettings GetSettings() => new JsonSerializerSettings().ConfigureForOperations();
    }
}