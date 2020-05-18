using Microsoft.Data.Sqlite;

namespace Annium.Core.DependencyInjection
{
    public static class SqliteConnectionExtensions
    {
        public static SqliteConnection InMemory(this SqliteConnection cn)
        {
            cn.ConnectionString = "Data Source=:memory:";

            return cn;
        }
    }
}