using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Jobs;
using Annium.Threading;
using Demo.Extensions.Jobs;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var scheduler = entry.Provider.Resolve<IScheduler>();
var job = entry.Provider.Resolve<Job>();
using var _ = scheduler.Schedule(() => job.Execute(), "*/2 * * * *");

Console.WriteLine("Start");
await entry.Ct;
Console.WriteLine("Wait 3 sec to teardown");
await Task.Delay(3000);
Console.WriteLine("Exit");