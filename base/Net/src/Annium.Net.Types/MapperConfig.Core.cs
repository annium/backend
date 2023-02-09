namespace Annium.Net.Types;

public static partial class MapperConfig
{
    static MapperConfig()
    {
        RegisterIgnored();
        RegisterArrays();
        RegisterRecords();
    }
}