﻿using System;
using System.Runtime.CompilerServices;

namespace Annium.Testing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BeforeAttribute : Attribute, ILocatedAttribute
    {
        public string File { get; }

        public int Line { get; }

        public string SetUpName { get; }

        public BeforeAttribute(string setUpName, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            File = file;
            Line = line;
            SetUpName = setUpName;
        }
    }
}