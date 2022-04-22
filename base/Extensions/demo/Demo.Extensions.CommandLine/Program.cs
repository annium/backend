using System;
using Annium.Core.Entrypoint;
using Annium.Extensions.CommandLine;

await using var entry = Entrypoint.Default.Setup();

Console.WriteLine("Hello from Demo.Extensions.Cli");
var pass = Cli.ReadSecure("Your pass: ");
Console.WriteLine($"Pass is !{pass}!");