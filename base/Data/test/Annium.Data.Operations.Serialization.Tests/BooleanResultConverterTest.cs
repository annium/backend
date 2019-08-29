using Annium.Testing;
using Newtonsoft.Json;

namespace Annium.Data.Operations.Serialization.Tests
{
    public class BooleanResultConverterTest
    {
        [Fact]
        public void BaseWrite_Blank_WritesCorrectly()
        {
            // act
            var str = JsonConvert.SerializeObject(Result.Success(), GetSettings());

            // assert
            str.IsEqual(@"{""isSuccess"":true,""isFailure"":false,""plainErrors"":[],""labeledErrors"":{}}");
        }

        [Fact]
        public void BaseWrite_WithErrors_WritesCorrectly()
        {
            // act
            var str = JsonConvert.SerializeObject(Result.Success().Error("plain").Error("label", "another"), GetSettings());

            // assert
            str.IsEqual(@"{""isSuccess"":false,""isFailure"":true,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
        }

        [Fact]
        public void BaseRead_Blank_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IBooleanResult>("{}", GetSettings());

            // assert
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
        }

        [Fact]
        public void BaseRead_Result_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IBooleanResult>(@"{""isSuccess"":true}", GetSettings());

            // assert
            result.IsSuccess.IsTrue();
            result.IsFailure.IsFalse();
        }

        [Fact]
        public void BaseRead_WithErrors_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IBooleanResult>(@"{""isSuccess"":true,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}", GetSettings());

            // assert
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("plain");
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At("label").At(0).IsEqual("another");
        }

        [Fact]
        public void DataWrite_Blank_WritesCorrectly()
        {
            // act
            var str = JsonConvert.SerializeObject(Result.Success(5), GetSettings());

            // assert
            str.IsEqual(@"{""isSuccess"":true,""isFailure"":false,""data"":5,""plainErrors"":[],""labeledErrors"":{}}");
        }

        [Fact]
        public void DataWrite_WithErrors_WritesCorrectly()
        {
            // act
            var str = JsonConvert.SerializeObject(Result.Success(5).Error("plain").Error("label", "another"), GetSettings());

            // assert
            str.IsEqual(@"{""isSuccess"":false,""isFailure"":true,""data"":5,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
        }

        [Fact]
        public void DataRead_BlankValueType_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IBooleanResult<int>>("{}", GetSettings());

            // assert
            result.Data.IsEqual(0);
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
        }

        [Fact]
        public void DataRead_BlankReferenceType_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IBooleanResult<string>>("{}", GetSettings());

            // assert
            result.Data.IsEqual(null);
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
        }

        [Fact]
        public void DataRead_Result_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IBooleanResult<int>>(@"{""data"":5,""isSuccess"":true}", GetSettings());

            // assert
            result.Data.IsEqual(5);
            result.IsSuccess.IsTrue();
            result.IsFailure.IsFalse();
        }

        [Fact]
        public void DataRead_WithErrors_ReadsCorrectly()
        {
            // act
            var result = JsonConvert.DeserializeObject<IBooleanResult<int>>(@"{""data"":5,""isSuccess"":true,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}", GetSettings());

            // assert
            result.Data.IsEqual(5);
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("plain");
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At("label").At(0).IsEqual("another");
        }

        private JsonSerializerSettings GetSettings() => new JsonSerializerSettings().ConfigureForOperations();
    }
}