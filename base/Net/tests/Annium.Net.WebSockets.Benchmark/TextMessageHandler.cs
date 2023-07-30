using System;

namespace Annium.Net.WebSockets.Benchmark;

public delegate void TextMessageHandler(ReadOnlySpan<byte> text);