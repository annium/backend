using System.Collections.Generic;

namespace Annium.linq2db.Extensions.Models
{
    public class Database
    {
        public IReadOnlyCollection<Table> Tables { get; }

        public Database(
            IReadOnlyCollection<Table> tables
        )
        {
            Tables = tables;
        }
    }
}