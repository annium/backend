using System;
using Annium.Testing;

namespace Annium.Diagnostics.CodeContracts.Tests
{
    public class ContractExtensionsTest
    {
        [Fact]
        public void IfNull_ThrowsArgumentNullException()
        {
            // arrange
            string value = null;

            // assert
            ((Action) (() => value.NotNull())).Throws<ArgumentNullException>();
        }

        [Fact]
        public void IfNotNull_ReturnsInstance()
        {
            // arrange
            var value = new Demo();

            // assert
            value.NotNull().Equals(value);
        }

        private class Demo { }
    }
}