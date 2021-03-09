using System;

namespace Annium.Data.Tables
{
    [Flags]
    public enum TablePermission
    {
        Init = 1 << 0,
        Add = 1 << 1,
        Update = 1 << 2,
        Remove = 1 << 3,
    }
}