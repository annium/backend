namespace Annium.Infrastructure.WebSockets.Server.Models;

public sealed record Unit
{
    public static Unit Default = new();

    private Unit()
    {
    }
}