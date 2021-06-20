using Annium.Logging.Shared.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal
{
    public class HelperTest
    {
        [Fact]
        public void Normal_Works()
        {
            // arrange
            var user = "alex";
            var time = "01.01.2021 08:00";

            // act
            var (message, data) = Helper.Process("Log-in by {user} at {time}", new[] { user, time });

            // assert
            message.Is("Log-in by alex at 01.01.2021 08:00");
            data.Has(2);
            data.At("user").As<string>().Is(user);
            data.At("time").As<string>().Is(time);
        }

        [Fact]
        public void Corner_Works()
        {
            // arrange
            var user = "alex";
            var time = "01.01.2021 08:00";

            // act
            var (message, data) = Helper.Process("{user}{time}", new[] { user, time });

            // assert
            message.Is("alex01.01.2021 08:00");
            data.Has(2);
            data.At("user").As<string>().Is(user);
            data.At("time").As<string>().Is(time);
        }
    }
}