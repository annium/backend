﻿namespace Annium.Testing
{
    public interface ILocatedAttribute
    {
        string File { get; }

        int Line { get; }
    }
}