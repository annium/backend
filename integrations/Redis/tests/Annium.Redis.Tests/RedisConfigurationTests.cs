using Annium.Testing;
using Xunit;

namespace Annium.Redis.Tests;

/// <summary>
/// Tests for <see cref="RedisConfiguration.GetConnectionString"/> covering host joining and
/// optional user/password suffix composition.
/// </summary>
public class RedisConfigurationTests
{
    /// <summary>
    /// Verifies multiple hosts are joined with a comma and no user/password suffix is appended
    /// when both are empty.
    /// </summary>
    [Fact]
    public void GetConnectionString_MultipleHosts_JoinsWithComma()
    {
        // arrange
        var config = new RedisConfiguration { Hosts = [new RedisHost("h1", 1), new RedisHost("h2", 2)] };

        // act
        var connectionString = config.GetConnectionString();

        // assert
        connectionString.Is("h1:1,h2:2");
    }

    /// <summary>
    /// Verifies a set User with an empty Password appends only the user suffix.
    /// </summary>
    [Fact]
    public void GetConnectionString_UserSetPasswordEmpty_AppendsUserOnly()
    {
        // arrange
        var config = new RedisConfiguration { Hosts = [new RedisHost("h1", 1)], User = "u" };

        // act
        var connectionString = config.GetConnectionString();

        // assert
        connectionString.Is("h1:1,user=u");
    }

    /// <summary>
    /// Verifies a set Password with an empty User appends only the password suffix.
    /// </summary>
    [Fact]
    public void GetConnectionString_PasswordSetUserEmpty_AppendsPasswordOnly()
    {
        // arrange
        var config = new RedisConfiguration { Hosts = [new RedisHost("h1", 1)], Password = "p" };

        // act
        var connectionString = config.GetConnectionString();

        // assert
        connectionString.Is("h1:1,password=p");
    }

    /// <summary>
    /// Verifies both User and Password set appends both suffixes, user before password.
    /// </summary>
    [Fact]
    public void GetConnectionString_UserAndPasswordSet_AppendsBothInOrder()
    {
        // arrange
        var config = new RedisConfiguration
        {
            Hosts = [new RedisHost("h1", 1)],
            User = "u",
            Password = "p",
        };

        // act
        var connectionString = config.GetConnectionString();

        // assert
        connectionString.Is("h1:1,user=u,password=p");
    }

    /// <summary>
    /// Verifies neither suffix is appended when both User and Password are empty.
    /// </summary>
    [Fact]
    public void GetConnectionString_UserAndPasswordEmpty_AppendsNeitherSuffix()
    {
        // arrange
        var config = new RedisConfiguration { Hosts = [new RedisHost("h1", 1)] };

        // act
        var connectionString = config.GetConnectionString();

        // assert
        connectionString.Is("h1:1");
    }
}
