using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Jobs
{
    public interface IScheduler
    {
        Action Schedule(Func<Task> handler, string interval);
    }
}