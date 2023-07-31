using System;

namespace Annium.Net.WebSockets;

public delegate void TextMessageHandler(ReadOnlySpan<byte> text);