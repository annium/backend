using Annium.Testing;

namespace Annium.Data.Operations.Tests
{
    public class StatusResultTest
    {
        [Fact]
        public void StatusResult_WithoutData_WorksCorrectly()
        {
            // arrange
            var result = Result.New(Access.Allowed);

            // assert
            result.Status.IsEqual(Access.Allowed);
        }

        [Fact]
        public void StatusResult_WithData_WorksCorrectly()
        {
            // arrange
            var result = Result.New(Access.Allowed, 5);
            var(status, data) = result;

            // assert
            result.Status.IsEqual(Access.Allowed);
            result.Data.IsEqual(5);
            status.IsEqual(Access.Allowed);
            data.IsEqual(5);
        }

        private enum Access
        {
            Allowed,
            Denied,
            Error
        }
    }
}