using System;
using System.Runtime.CompilerServices;

namespace Annium.Testing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AfterAttribute : Attribute, ILocatedAttribute
    {
        public string File { get; }

        public int Line { get; }

        public string TearDownName { get; }

        public AfterAttribute(string tearDownName, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            File = file;
            Line = line;
            TearDownName = tearDownName;
        }
    }
}