using System.Threading.Tasks;

namespace Annium.Extensions.Execution.Tests;

internal static class Helper
{
    public static Task AsyncLongWork() => AsyncWork(200);
    public static Task AsyncFastWork() => AsyncWork(20);

    private static async Task AsyncWork(int iterations)
    {
        var i = 0;
        while (i < iterations)
        {
            await Task.Delay(2);
            i++;
        }
    }
}