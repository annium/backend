using System;
using System.IO;
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

        [Fact]
        public void Trace_ContainsTypeAndId()
        {
            // arrange
            var stdout = Console.Out;
            var writer = new StringWriter();
            var a = new Demo();

            // act
            Console.SetOut(writer);
            a.Trace("sample");
            Console.SetOut(stdout);
            var output = writer.ToString();
            writer.Dispose();

            // assert
            output.Contains(a.GetId()).IsTrue();
            output.Contains(a.GetType().Name).IsTrue();
            output.Contains("sample").IsTrue();
        }

        private class Demo
        {
        }
    }
}