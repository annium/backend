using System;

namespace Annium.Extensions.Mapper
{
    public class MappingException : Exception
    {
        public MappingException(Type src, Type tgt, params string[] messages):
            base($"Can't convert {src.FullName} -> {tgt.FullName}. {string.Join(Environment.NewLine,messages)}") { }
    }
}