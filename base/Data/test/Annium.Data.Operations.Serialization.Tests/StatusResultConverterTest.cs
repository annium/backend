using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Testing;

namespace Annium.Data.Operations.Serialization.Tests
{
    public class StatusResultConverterTest
    {
        [Fact]
        public void BaseWrite_Blank_WritesCorrectly()
        {
            // act
            var str = JsonSerializer.Serialize(Result.Status(5), GetSettings());

            // assert
            str.IsEqual(@"{""status"":5,""plainErrors"":[],""labeledErrors"":{}}");
        }

        [Fact]
        public void BaseWrite_WithErrors_WritesCorrectly()
        {
            // act
            var str = JsonSerializer.Serialize(Result.Status(5).Error("plain").Error("label", "another"), GetSettings());

            // assert
            str.IsEqual(@"{""status"":5,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
        }

        [Fact]
        public void BaseRead_BlankValueType_ReadsCorrectly()
        {
            // act
            var result = JsonSerializer.Deserialize<IStatusResult<int>>("{}", GetSettings());

            // assert
            result.Status.IsEqual(0);
        }

        [Fact]
        public void BaseRead_BlankReferenceType_ReadsCorrectly()
        {
            // act
            var result = JsonSerializer.Deserialize<IStatusResult<string>>("{}", GetSettings());

            // assert
            result.Status.IsEqual(null!);
        }

        [Fact]
        public void BaseRead_Result_ReadsCorrectly()
        {
            // act
            var result = JsonSerializer.Deserialize<IStatusResult<int>>(@"{""status"":5}", GetSettings());

            // assert
            result.Status.IsEqual(5);
        }

        [Fact]
        public void BaseRead_WithErrors_ReadsCorrectly()
        {
            // act
            var result = JsonSerializer.Deserialize<IStatusResult<int>>(@"{""status"":5,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}", GetSettings());

            // assert
            result.Status.IsEqual(5);
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("plain");
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At("label").At(0).IsEqual("another");
        }

        [Fact]
        public void DataWrite_Blank_WritesCorrectly()
        {
            // act
            var str = JsonSerializer.Serialize(Result.Status(5, "some"), GetSettings());

            // assert
            str.IsEqual(@"{""status"":5,""data"":""some"",""plainErrors"":[],""labeledErrors"":{}}");
        }

        [Fact]
        public void DataWrite_WithErrors_WritesCorrectly()
        {
            // act
            var str = JsonSerializer.Serialize(Result.Status(5, "some").Error("plain").Error("label", "another"), GetSettings());

            // assert
            str.IsEqual(@"{""status"":5,""data"":""some"",""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
        }

        [Fact]
        public void DataRead_BlankValueType_ReadsCorrectly()
        {
            // act
            var result = JsonSerializer.Deserialize<IStatusResult<int, int>>("{}", GetSettings());

            // assert
            result.Status.IsEqual(0);
            result.Data.IsEqual(0);
        }

        [Fact]
        public void DataRead_BlankReferenceType_ReadsCorrectly()
        {
            // act
            var result = JsonSerializer.Deserialize<IStatusResult<string, string>>("{}", GetSettings());

            // assert
            result.Status.IsEqual(null!);
            result.Data.IsEqual(null!);
        }

        [Fact]
        public void DataRead_Result_ReadsCorrectly()
        {
            // act
            var result = JsonSerializer.Deserialize<IStatusResult<int, string>>(@"{""status"":5,""data"":""some""}", GetSettings());

            // assert
            result.Status.IsEqual(5);
            result.Data.IsEqual("some");
        }

        [Fact]
        public void DataRead_WithErrors_ReadsCorrectly()
        {
            // act
            var result = JsonSerializer.Deserialize<IStatusResult<int, string>>(@"{""status"":5,""data"":""some"",""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}", GetSettings());

            // assert
            result.Status.IsEqual(5);
            result.Data.IsEqual("some");
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("plain");
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At("label").At(0).IsEqual("another");
        }

        private JsonSerializerOptions GetSettings() => new JsonSerializerOptions().ConfigureForOperations();
    }
}