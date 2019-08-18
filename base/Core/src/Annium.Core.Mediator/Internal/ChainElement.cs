using System;

namespace Annium.Core.Mediator.Internal
{
    internal class ChainElement
    {
        public Type Handler { get; }
        public Type RequestIn { get; }
        public Type RequestOut { get; }
        public Type ResponseIn { get; }
        public Type ResponseOut { get; }

        public ChainElement(
            Type implementation,
            Type requestIn,
            Type requestOut,
            Type responseIn,
            Type responseOut
        )
        {
            Handler = implementation;
            RequestIn = requestIn;
            RequestOut = requestOut;
            ResponseIn = responseIn;
            ResponseOut = responseOut;
        }
    }
}