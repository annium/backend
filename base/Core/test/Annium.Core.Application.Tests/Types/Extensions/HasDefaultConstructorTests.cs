using System;
using System.Collections;
using System.IO;
using Annium.Testing;

namespace Annium.Core.Application.Tests.Types.Extensions
{
    public class HasDefaultConstructorTests
    {
        [Fact]
        public void HasDefaultConstructor_OfNull_Throws()
        {
            //assert
            ((Action) (() => (null as Type).HasDefaultConstructor())).Throws<ArgumentNullException>();
        }

        [Fact]
        public void HasDefaultConstructor_Class_Works()
        {
            //assert
            typeof(object).HasDefaultConstructor().IsTrue();
            typeof(FileInfo).HasDefaultConstructor().IsFalse();
        }

        [Fact]
        public void HasDefaultConstructor_Struct_Works()
        {
            //assert
            typeof(long).HasDefaultConstructor().IsTrue();
            typeof(ValueTuple<>).HasDefaultConstructor().IsFalse();
        }

        [Fact]
        public void HasDefaultConstructor_Other_Throws()
        {
            //assert
            ((Action) (() => typeof(IEnumerable).HasDefaultConstructor())).Throws<ArgumentException>();
        }
    }
}