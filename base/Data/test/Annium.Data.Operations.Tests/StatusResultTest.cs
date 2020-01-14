using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Tests
{
    public class StatusResultTest
    {
        [Fact]
        public void StatusResult_WithoutData_WorksCorrectly()
        {
            // arrange
            var result = Result.Status(Access.Allowed);

            // assert
            result.Status.IsEqual(Access.Allowed);
        }

        [Fact]
        public void StatusResult_WithData_WorksCorrectly()
        {
            // arrange
            var result = Result.Status(Access.Allowed, 5);
            var(status, data) = result;

            // assert
            result.Status.IsEqual(Access.Allowed);
            result.Data.IsEqual(5);
            status.IsEqual(Access.Allowed);
            data.IsEqual(5);
        }

        [Fact]
        public void StatusResult_Clone_ReturnsValidClone()
        {
            // arrange
            var succeed = Result.Status(Access.Allowed);
            var failed = Result.Status(Access.Denied).Error("plain").Error("label", "value");

            // act
            var succeedClone = succeed.Clone();
            var failedClone = failed.Clone();

            // assert
            succeedClone.Status.IsEqual(Access.Allowed);
            succeedClone.HasErrors.IsFalse();
            failedClone.Status.IsEqual(Access.Denied);
            failedClone.HasErrors.IsTrue();
            failedClone.PlainErrors.Has(1);
            failedClone.PlainErrors.At(0).IsEqual("plain");
            failedClone.LabeledErrors.Has(1);
            failedClone.LabeledErrors.At("label").Has(1);
            failedClone.LabeledErrors.At("label").At(0).IsEqual("value");
        }

        [Fact]
        public void StatusResult_CloneWithData_ReturnsValidClone()
        {
            // arrange
            var succeed = Result.Status(Access.Allowed, "welcome");
            var failed = Result.Status(Access.Denied, "goodbye").Error("plain").Error("label", "value");

            // act
            var succeedClone = succeed.Clone();
            var failedClone = failed.Clone();

            // assert
            succeedClone.Status.IsEqual(Access.Allowed);
            succeedClone.HasErrors.IsFalse();
            succeedClone.Data.IsEqual("welcome");
            failedClone.Status.IsEqual(Access.Denied);
            failedClone.HasErrors.IsTrue();
            failedClone.PlainErrors.Has(1);
            failedClone.PlainErrors.At(0).IsEqual("plain");
            failedClone.LabeledErrors.Has(1);
            failedClone.LabeledErrors.At("label").Has(1);
            failedClone.LabeledErrors.At("label").At(0).IsEqual("value");
            failedClone.Data.IsEqual("goodbye");
        }

        private enum Access
        {
            Allowed,
            Denied,
            Error
        }
    }
}