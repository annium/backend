using Annium.Testing;

namespace Annium.Data.Operations.Tests
{
    public class BooleanResultTest
    {
        [Fact]
        public void BooleanResult_Success_IsCorrect()
        {
            // arrange
            var result = Result.Success();

            // assert
            result.IsSuccess.IsTrue();
            result.IsFailure.IsFalse();
        }

        [Fact]
        public void BooleanResult_SuccessWithError_IsFailure()
        {
            // arrange
            var result = Result.Success().Error("plain");

            // assert
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
        }

        [Fact]
        public void BooleanResult_Failure_IsCorrect()
        {
            // arrange
            var result = Result.Failure();

            // assert
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
        }

        [Fact]
        public void BooleanResult_SuccessWithData_IsCorrect()
        {
            // arrange
            var result = Result.Success(5);
            var(succeed, data) = result;

            // assert
            result.Data.IsEqual(data);
            result.IsSuccess.IsTrue();
            result.IsFailure.IsFalse();
            data.IsEqual(5);
            succeed.IsTrue();
        }

        [Fact]
        public void BooleanResult_SuccessWithDataWithError_IsFailure()
        {
            // arrange
            var result = Result.Success(5).Error("plain");
            var(succeed, data) = result;

            // assert
            result.Data.IsEqual(data);
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
            data.IsEqual(5);
            succeed.IsFalse();
        }

        [Fact]
        public void BooleanResult_FailureWithData_IsCorrect()
        {
            // arrange
            var result = Result.Failure(5);
            var(succeed, data) = result;

            // assert
            result.Data.IsEqual(data);
            result.IsSuccess.IsFalse();
            result.IsFailure.IsTrue();
            data.IsEqual(5);
            succeed.IsFalse();
        }
    }
}