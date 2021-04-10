using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution
{
    public interface IBatchExecutor
    {
        IBatchExecutor With(Action handler);
        IBatchExecutor With(Func<Task> handler);
    }
}