set shell := ["bash", "-cu"]
set positional-arguments
# lib.just is copied in by the umbrella repo's `just copy-ci`; recipes redefined below
# override the shared ones.
set allow-duplicate-recipes := true

import 'lib.just'

# overrides

test:
    @echo "=== $0 ==="
    dotnet test -c Release --no-build --nologo --logger "trx;LogFilePrefix=test-results.trx"

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
