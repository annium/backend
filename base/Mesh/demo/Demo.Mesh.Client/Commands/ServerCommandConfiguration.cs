using System;
using Annium.Extensions.Arguments;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

public record ServerCommandConfiguration
{
    [Option("s", isRequired: true)]
    [Help("Server address.")]
    public Uri Server { get; set; } = default!;
}