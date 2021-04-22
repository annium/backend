using Annium.Testing;
using Xunit;

namespace Annium.Diagnostics.Debug.Tests
{
    public class IdExtensionsTest
    {
        [Fact]
        public void GetId_IsStablyUniquePerObject()
        {
            // arrange
            var a = new object();
            var b = new object();

            // assert
            string.IsNullOrWhiteSpace(a.GetId()).IsFalse();
            (a.GetId() == a.GetId()).IsTrue();
            (a.GetId() != b.GetId()).IsTrue();
        }

        private class Demo
        {
        }
    }
}