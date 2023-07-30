using System;

namespace Annium.Net.WebSockets.Benchmark;

public delegate void BinaryMessageHandler(ReadOnlyMemory<byte> data);