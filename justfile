set shell := ["bash", "-cu"]
set positional-arguments
# lib.just is copied in by the umbrella repo's `just copy-ci`; recipes redefined below
# override the shared ones.
set allow-duplicate-recipes := true

import 'lib.just'

# overrides

# Every suite here is container-backed (Kafka, RabbitMQ, NATS, Redis, Mongo, Postgres, S3), so test
# modules run one at a time. MTP parallelises modules by default, unlike the VSTest runner this
# replaced; on a CI runner that starves the containers and Npgsql times out opening connections.
test:
    @echo "=== $0 ==="
    dotnet test -c Release --no-build --report-xunit-trx --max-parallel-test-modules 1

# load (MessageBus throughput / zero-loss / ordering harness; needs Docker; NOT part of CI)

load-kafka:
    @echo "=== $0 ==="
    dotnet run -c Release --project base/MessageBus/tests/Annium.MessageBus.Kafka.Load

load-rabbitmq:
    @echo "=== $0 ==="
    dotnet run -c Release --project base/MessageBus/tests/Annium.MessageBus.RabbitMq.Load

load-nats:
    @echo "=== $0 ==="
    dotnet run -c Release --project base/MessageBus/tests/Annium.MessageBus.Nats.Load

load: load-kafka load-rabbitmq load-nats
