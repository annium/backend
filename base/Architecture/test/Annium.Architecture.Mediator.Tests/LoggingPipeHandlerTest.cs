using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests
{
    public class LoggingPipeHandlerTest : TestBase
    {
        [Fact]
        public async Task ReturnsOriginalResult()
        {
            // arrange
            var mediator = GetMediator(cfg => cfg.AddLoggingHandler().AddHandler(typeof(EchoRequestHandler<>)));
            var request = new LoginRequest();

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