using Annium.Core.Runtime.Types;
using Xunit;

[assembly: AutoScanned]
// The whole run shares one Kafka broker and reuses subjects across tests; run serially to avoid cross-test interference.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
