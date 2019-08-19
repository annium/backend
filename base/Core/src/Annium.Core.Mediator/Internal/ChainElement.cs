using System;

namespace Annium.Core.Mediator.Internal
{
    internal class ChainElement
    {
        public Type Handler { get; }
        public ValueTuple<Type, Type> Output { get; }

        public ChainElement(
            Type handler
        )
        {
            Handler = handler;
        }

        public ChainElement(
            Type handler,
            ValueTuple<Type, Type> output
        ) : this(handler)
        {
            Output = output;
        }
    }
}