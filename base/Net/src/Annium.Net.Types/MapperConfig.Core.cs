namespace Annium.Net.Types;

public static partial class MapperConfig
{
    static MapperConfig()
    {
        RegisterBaseTypes();
        RegisterIgnored();
        RegisterArrays();
        RegisterRecords();
    }
}