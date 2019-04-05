using System;

namespace Annium.Extensions.Conversion
{
    internal class ConverterInstance<TSource, TTarget> : IConverterInstance
    {
        public Type Source { get; }

        public Type Target { get; }

        public Func<TSource, TTarget> Convert;

        public ConverterInstance(Func<TSource, TTarget> converter)
        {
            Source = typeof(TSource);
            Target = typeof(TTarget);
            Convert = converter;
        }
    }

    internal interface IConverterInstance
    {
        Type Source { get; }

        Type Target { get; }
    }
}