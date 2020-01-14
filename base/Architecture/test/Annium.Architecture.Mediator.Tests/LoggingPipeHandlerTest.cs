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
            var mediator = GetMediator(cfg => cfg.AddLoggingHandler().Add(typeof(EchoRequestHandler<>)));
            var request = new LoginRequest();

            // act
            var result = await mediator.SendAsync<LoginRequest, IStatusResult<OperationStatus, LoginRequest>>(request);

            // assert
            result.Status.IsEqual(OperationStatus.OK);
            result.HasErrors.IsFalse();
        }

        private class LoginRequest : IThrowing
        {
            public bool Throw { get; set; }
        }
    }
}