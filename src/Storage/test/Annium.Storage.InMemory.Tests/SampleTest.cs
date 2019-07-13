using Annium.Testing;

namespace Annium.Storage.InMemory.Tests
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