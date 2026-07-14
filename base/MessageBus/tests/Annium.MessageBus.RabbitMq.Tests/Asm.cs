using Annium.Core.Runtime.Types;
using Xunit;

[assembly: AutoScanned]
// The whole run shares one RabbitMQ broker; run serially to avoid cross-test interference on shared topology.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
