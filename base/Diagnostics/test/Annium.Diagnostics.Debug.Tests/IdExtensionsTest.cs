using Annium.Testing;
using Xunit;

namespace Annium.Diagnostics.Debug.Tests
{
    public class IdExtensionsTest
    {
        [Fact]
        public void GetId_IsStablyUniquePerObject_NotSharedAmongTypes()
        {
            // arrange
            var a = new object();
            var b = new object();
            var c = new { };

            // assert
            string.IsNullOrWhiteSpace(a.GetId()).IsFalse();
            (a.GetId() == a.GetId()).IsTrue();
            a.GetId().Is("001");
            b.GetId().Is("002");
            c.GetId().Is("001");
        }

        private class Demo
        {
        }
    }
}