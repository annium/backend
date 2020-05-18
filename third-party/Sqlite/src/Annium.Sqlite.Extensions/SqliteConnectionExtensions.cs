using Microsoft.Data.Sqlite;

namespace Annium.Sqlite.Extensions
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