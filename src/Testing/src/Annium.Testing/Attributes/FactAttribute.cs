using System;
using System.Runtime.CompilerServices;

namespace Annium.Testing
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FactAttribute : Attribute, ILocatedAttribute
    {
        public string File { get; }

        public int Line { get; }

        public FactAttribute([CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            File = file;
            Line = line;
        }
    }
}