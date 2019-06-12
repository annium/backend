using Annium.Testing;

namespace Annium.Extensions.Validation.Tests
{
    public class SampleTest
    {
        [Fact]
        public void True_IsTrue()
        {
            // arrange
            var value = true;

            // assert
            value.IsTrue();
        }
    }
}