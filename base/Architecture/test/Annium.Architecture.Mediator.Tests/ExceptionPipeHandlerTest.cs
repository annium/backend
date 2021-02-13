using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests
{
    public class ExceptionPipeHandlerTest : TestBase
    {
        [Fact]
        public async Task Exception_ReturnsUncaughtExceptionResult()
        {
            // arrange
            var mediator = GetMediator(cfg => cfg.AddExceptionHandler().Add(typeof(EchoRequestHandler<>)));
            var request = new LoginRequest {Throw = true};

            // act
            var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

            // assert
            result.Status.IsEqual(OperationStatus.UncaughtException);
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("TEST EXCEPTION");
        }

        [Fact]
        public async Task Success_ReturnsOriginalResult()
        {
            // arrange
            var mediator = GetMediator(cfg => cfg.AddCompositionHandler().Add(typeof(EchoRequestHandler<>)));
            var request = new LoginRequest {Throw = false};

            // act
            var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

            // assert
            result.Status.IsEqual(OperationStatus.Ok);
            result.IsOk.IsTrue();
        }

        private class LoginRequest : IThrowing
        {
            public bool Throw { get; set; }
        }
    }
}