using Annium.Core.Runtime.Types;
using Xunit.Sdk;
using Xunit.v3;

[assembly: AutoScanned]
// The whole run shares one RabbitMQ broker; run serially to avoid cross-test interference on shared topology.
[assembly: Parallelization(Mode = ParallelMode.None)]
