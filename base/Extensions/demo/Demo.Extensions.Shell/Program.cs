using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Shell;
using Demo.Extensions.Shell;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;

var shell = provider.Resolve<IShell>();
var ls = await shell
    .Cmd("ls")
    .Configure(x => x.WorkingDirectory = "/")
    .RunAsync(ct);

Console.WriteLine(ls.IsSuccess);
Console.WriteLine(ls.Code);
Console.WriteLine(ls.Output);
Console.WriteLine(ls.Error);