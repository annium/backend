using System;

namespace Annium.Net.WebSockets;

public delegate void BinaryMessageHandler(ReadOnlySpan<byte> data);