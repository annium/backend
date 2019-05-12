using System;
using System.Linq.Expressions;
using Annium.Testing;

namespace Annium.Extensions.Mapper.Tests
{
    public class RepackerTest
    {
        [Fact]
        public void Lambda_Works()
        {
            // act
            var result = repack<int, int>(v => v);

            // assert
            result(1).IsEqual(1);
        }

        [Fact]
        public void Call_Works()
        {
            // act
            var result = repack<int, string>(v => v.ToString());

            // assert
            result(1).IsEqual("1");
        }

        [Fact]
        public void Member_Works()
        {
            // act
            var result = repack<string, int>(v => v.Length);

            // assert
            result("asd").IsEqual(3);
        }

        [Fact]
        public void New_Works()
        {
            // act
            var result = repack<char, string>(v => new string(v, 5));

            // assert
            result('a').IsEqual("aaaaa");
        }

        private Func<S, R> repack<S, R>(Expression<Func<S, R>> ex)
        {
            var repacker = new Repacker();
            var param = Expression.Parameter(typeof(S));

            return ((Expression<Func<S, R>>) repacker.Repack(ex) (param)).Compile();
        }
    }
}