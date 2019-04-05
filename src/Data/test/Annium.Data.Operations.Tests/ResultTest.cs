using System.Collections.Generic;
using Annium.Testing;

namespace Annium.Data.Operations.Tests
{
    public class ResultTest
    {
        [Fact]
        public void Blank_HasNoErrors()
        {
            // arrange
            var result = Result.New();

            // assert
            result.HasErrors.IsFalse();
        }

        [Fact]
        public void PlainError_IsAddedToPlainErrors()
        {
            // arrange
            var result = Result.New();

            // act
            result.Error("plain");

            // assert
            result.HasErrors.IsTrue();
            result.PlainErrors.Count.IsEqual(1);
            result.PlainErrors.At(0).IsEqual("plain");
        }

        [Fact]
        public void LabeledError_IsAddedToLabeledErrors()
        {
            // arrange
            var result = Result.New();

            // act
            result.Error("label", "plain");

            // assert
            result.HasErrors.IsTrue();
            result.LabeledErrors.Count.IsEqual(1);
            result.LabeledErrors.At("label").IsEqual("plain");
        }

        [Fact]
        public void PlainErrors_Params_IsAddedCorrectly()
        {
            // arrange
            var result = Result.New();

            // act
            result.Errors("plain", "another", "another");

            // assert
            result.PlainErrors.Count.IsEqual(2);
            result.PlainErrors.At(0).IsEqual("plain");
            result.PlainErrors.At(1).IsEqual("another");
        }

        [Fact]
        public void PlainErrors_Collection_IsAddedCorrectly()
        {
            // arrange
            var result = Result.New();

            // act
            result.Errors(new List<string>() { "plain", "another", "another" });

            // assert
            result.PlainErrors.Count.IsEqual(2);
            result.PlainErrors.At(0).IsEqual("plain");
            result.PlainErrors.At(1).IsEqual("another");
        }

        [Fact]
        public void LabeledErrors_Params_IsAddedCorrectly()
        {
            // arrange
            var result = Result.New();

            // act
            result.Errors(("label", "plain"), ("other", "prev"), ("other", "another"));

            // assert
            result.LabeledErrors.Count.IsEqual(2);
            result.LabeledErrors.At("label").IsEqual("plain");
            result.LabeledErrors.At("other").IsEqual("another");
        }

        [Fact]
        public void LabeledErrors_Collection_IsAddedCorrectly()
        {
            // arrange
            var result = Result.New();

            // act
            result.Errors(new Dictionary<string, string>() { { "label", "plain" }, { "other", "another" } });

            // assert
            result.LabeledErrors.Count.IsEqual(2);
            result.LabeledErrors.At("label").IsEqual("plain");
            result.LabeledErrors.At("other").IsEqual("another");
        }

        [Fact]
        public void Join_Params_IsDoneCorrectly()
        {
            // arrange
            var result = Result.New().Error("own").Error("label", "mine");
            var plain = Result.New().Errors("plain", "another");
            var labeled = Result.New().Errors(("a", "va"), ("b", "vb"));

            // act
            result.Join(plain, labeled);

            // assert
            result.HasErrors.IsTrue();
            result.PlainErrors.Count.IsEqual(3);
            result.PlainErrors.At(0).IsEqual("own");
            result.PlainErrors.At(1).IsEqual("plain");
            result.PlainErrors.At(2).IsEqual("another");
            result.LabeledErrors.Count.IsEqual(3);
            result.LabeledErrors.At("label").IsEqual("mine");
            result.LabeledErrors.At("a").IsEqual("va");
            result.LabeledErrors.At("b").IsEqual("vb");
        }

        [Fact]
        public void Join_Collection_IsDoneCorrectly()
        {
            // arrange
            var result = Result.New().Error("own").Error("label", "mine");
            var plain = Result.New().Errors("plain", "another");
            var labeled = Result.New().Errors(("a", "va"), ("b", "vb"));

            // act
            result.Join(new List<IResult>() { plain, labeled });

            // assert
            result.HasErrors.IsTrue();
            result.PlainErrors.Count.IsEqual(3);
            result.PlainErrors.At(0).IsEqual("own");
            result.PlainErrors.At(1).IsEqual("plain");
            result.PlainErrors.At(2).IsEqual("another");
            result.LabeledErrors.Count.IsEqual(3);
            result.LabeledErrors.At("label").IsEqual("mine");
            result.LabeledErrors.At("a").IsEqual("va");
            result.LabeledErrors.At("b").IsEqual("vb");
        }
    }
}