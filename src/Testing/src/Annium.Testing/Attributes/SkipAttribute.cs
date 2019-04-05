using System;
using System.Runtime.CompilerServices;

namespace Annium.Testing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SkipAttribute : Attribute, ILocatedAttribute
    {
        public string File { get; }

        public int Line { get; }

        public SkipAttribute([CallerFilePath] string file = "", [CallerLineNumber] int line = 0) { }
    }
}