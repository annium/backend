using System;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Core.Mapper.Tests
{
    public class RepackerTest
    {
        [Fact]
        public void Binary_Works()
        {
            // act
            var result = Repack<int?, int>(v => v ?? default);

            // assert
            result(1).IsEqual(1);
            result(null).IsEqual(0);
        }

        [Fact]
        public void Call_Works()
        {
            // act
            var result = Repack<int, string>(v => v.ToString());

            // assert
            result(1).IsEqual("1");
        }

        [Fact]
        public void Lambda_Works()
        {
            // act
            var result = Repack<int, int>(v => v);

            // assert
            result(1).IsEqual(1);
        }

        [Fact]
        public void Member_Works()
        {
            // act
            var result = Repack<string, int>(v => v.Length);

            // assert
            result("asd").IsEqual(3);
        }

        [Fact]
        public void MemberInit_Works()
        {
            // act
            var result = Repack<int, string>(v => new string(' ', v));

            // assert
            result(3).IsEqual("   ");
        }

        [Fact]
        public void Switch_Works()
        {
            // act
            var result = Repack<string, string>(v =>
                v == "1" ? "one" :
                v == "2" ? "two" :
                "other"
            );

            // assert
            result("1").IsEqual("one");
            result("2").IsEqual("two");
            result("3").IsEqual("other");
        }

        [Fact]
        public void New_Works()
        {
            // act
            var result = Repack<string, Exception>(v => new Exception(v) { Source = v });

            // assert
            var ex = result("a");
            ex.Message.IsEqual("a");
            ex.Source.IsEqual("a");
        }

        [Fact]
        public void Unary_Works()
        {
            // act
            var result = Repack<bool, bool>(v => !v);

            // assert
            result(false).IsTrue();
        }

        private Func<S, R> Repack<S, R>(Expression<Func<S, R>> ex)
        {
            var repacker = new ServiceCollection()
                .AddRuntimeTools(Assembly.GetCallingAssembly(), false)
                .AddMapper(false)
                .BuildServiceProvider()
                .GetRequiredService<IRepacker>();

            var param = Expression.Parameter(typeof(S));

            return ((Expression<Func<S, R>>) repacker.Repack(ex)(param)).Compile();
        }
    }
}