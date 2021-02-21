using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests
{
    public class ValidationPipeHandlerTest : TestBase
    {
        [Fact]
        public async Task ValidationFailure_ReturnsBadRequest()
        {
            // arrange
            var mediator = GetMediator(cfg => cfg.AddValidationHandler().AddHandler(typeof(EchoRequestHandler<>)));
            var request = new LoginRequest();

            // act
            var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

            // assert
            result.Status.IsEqual(OperationStatus.BadRequest);
            result.LabeledErrors.Has(2);
            result.LabeledErrors.At(nameof(LoginRequest.UserName)).Has(1);
            result.LabeledErrors.At(nameof(LoginRequest.Password)).Has(1);
        }

        [Fact]
        public async Task ValidationSuccess_ReturnsOriginalResult()
        {
            // arrange
            var mediator = GetMediator(cfg => cfg.AddValidationHandler().AddHandler(typeof(EchoRequestHandler<>)));
            var request = new LoginRequest {UserName = "user", Password = "pass"};

            // act
            var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

            // assert
            result.Status.IsEqual(OperationStatus.Ok);
            result.IsOk.IsTrue();
        }

        private class LoginRequest : IUserName, IPassword, IThrowing
        {
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public bool Throw { get; } = false;
        }

        private interface IUserName
        {
            string UserName { get; }
        }

        private interface IPassword
        {
            string Password { get; }
        }

        private class UserNameValidator : Validator<IUserName>
        {
            public UserNameValidator()
            {
                Field(e => e.UserName).Required();
            }
        }

        private class PasswordValidator : Validator<IPassword>
        {
            public PasswordValidator()
            {
                Field(e => e.Password).Required();
            }
        }
    }
}