using System;

namespace Annium.Net.WebSockets.Benchmark;

public delegate void BinaryMessageHandler(ReadOnlySpan<byte> data);