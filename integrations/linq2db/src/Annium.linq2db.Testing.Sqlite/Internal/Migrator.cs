using System;
using System.Reflection;
using DbUp;
using DbUp.Engine;

namespace Annium.linq2db.Testing.Sqlite.Internal;

internal static class Migrator
{
    public static void Execute(Assembly assembly, string connectionString)
    {
        DeployChanges.To
            .SQLiteDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(assembly, x => x.Contains(".Migrations."))
            .WithTransactionPerScript()
            .LogToConsole()
            .Build()
            .PerformUpgrade()
            .AssertResult();
    }

    private static void AssertResult(this DatabaseUpgradeResult result)
    {
        if (!result.Successful)
            throw new ApplicationException($"{result.ErrorScript}: {result.Error}");
    }
}