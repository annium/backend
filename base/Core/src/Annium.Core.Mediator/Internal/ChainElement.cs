using System;

namespace Annium.Core.Mediator.Internal
{
    internal class ChainElement
    {
        public Type Handler { get; }
        public NextPrototype Next { get; }

        public ChainElement(
            Type handler
        )
        {
            Handler = handler;
        }

        public ChainElement(
            Type handler,
            NextPrototype next
        ) : this(handler)
        {
            Next = next;
        }

        public override string ToString() => Handler.ToString();
    }
}