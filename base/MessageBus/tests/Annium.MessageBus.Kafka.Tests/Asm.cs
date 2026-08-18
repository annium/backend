using Annium.Core.Runtime.Types;
using Xunit.Sdk;
using Xunit.v3;

[assembly: AutoScanned]
// The whole run shares one Kafka broker and reuses subjects across tests; run serially to avoid cross-test interference.
[assembly: Parallelization(Mode = ParallelMode.None)]
