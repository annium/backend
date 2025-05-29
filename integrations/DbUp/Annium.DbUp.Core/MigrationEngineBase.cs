using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp.Builder;

namespace Annium.DbUp.Core;

public abstract class MigrationEngineBase<T>
    where T : MigrationEngineBase<T>
{
    protected readonly UpgradeEngineBuilder InitBuilder;
    protected readonly UpgradeEngineBuilder MigrationsBuilder;

    protected MigrationEngineBase(UpgradeEngineBuilder initBuilder, UpgradeEngineBuilder migrationsBuilder)
    {
        InitBuilder = initBuilder.WithTransactionPerScript().WithVariablesEnabled().LogToConsole();
        MigrationsBuilder = migrationsBuilder.WithTransactionPerScript().WithVariablesEnabled().LogToConsole();
    }

    public T WithScriptsFromDirectory(string folder)
    {
        InitBuilder.WithScriptsFromFileSystem(Path.Combine(folder, "Scripts", "Init"));
        MigrationsBuilder.WithScriptsFromFileSystem(Path.Combine(folder, "Scripts", "Migrations"));

        return (T)this;
    }

    public T WithScriptsFromAssembly(Assembly assembly)
    {
        InitBuilder.WithScriptsEmbeddedInAssembly(assembly, x => x.Contains(".Scripts.Init."));
        MigrationsBuilder.WithScriptsEmbeddedInAssembly(assembly, x => x.Contains(".Scripts.Migrations."));

        return (T)this;
    }

    public T WithVariable(string name, string value)
    {
        InitBuilder.WithVariable(name, value);
        MigrationsBuilder.WithVariable(name, value);

        return (T)this;
    }

    public T WithVariables(IReadOnlyDictionary<string, string> variables)
    {
        var vars = variables.ToDictionary();
        InitBuilder.WithVariables(vars);
        MigrationsBuilder.WithVariables(vars);

        return (T)this;
    }

    public void Execute()
    {
        ExecuteBuilder(InitBuilder);
        ExecuteBuilder(MigrationsBuilder);

        static void ExecuteBuilder(UpgradeEngineBuilder builder)
        {
            var result = builder.Build().PerformUpgrade();
            if (!result.Successful)
                throw new ApplicationException($"{result.ErrorScript}: {result.Error}");
        }
    }
}
