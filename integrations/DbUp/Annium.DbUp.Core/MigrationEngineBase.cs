using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp.Builder;

namespace Annium.DbUp.Core;

/// <summary>
/// Abstract base class for database migration engines that provides a fluent API for configuring and executing database migrations.
/// Supports separate initialization and migration script execution.
/// </summary>
/// <typeparam name="T">The concrete migration engine type for fluent method chaining</typeparam>
public abstract class MigrationEngineBase<T>
    where T : MigrationEngineBase<T>
{
    /// <summary>
    /// The upgrade engine builder for initialization scripts
    /// </summary>
    protected readonly UpgradeEngineBuilder InitBuilder;

    /// <summary>
    /// The upgrade engine builder for migration scripts
    /// </summary>
    protected readonly UpgradeEngineBuilder MigrationsBuilder;

    /// <summary>
    /// Initializes a new instance of the migration engine with the specified builders
    /// </summary>
    /// <param name="initBuilder">The upgrade engine builder for initialization scripts</param>
    /// <param name="migrationsBuilder">The upgrade engine builder for migration scripts</param>
    protected MigrationEngineBase(UpgradeEngineBuilder initBuilder, UpgradeEngineBuilder migrationsBuilder)
    {
        InitBuilder = initBuilder.WithTransactionPerScript().WithVariablesEnabled().LogToConsole();
        MigrationsBuilder = migrationsBuilder.WithTransactionPerScript().WithVariablesEnabled().LogToConsole();
    }

    /// <summary>
    /// Configures the migration engine to load scripts from a directory structure
    /// </summary>
    /// <param name="folder">The root folder containing Scripts/Init and Scripts/Migrations subdirectories</param>
    /// <returns>The migration engine instance for method chaining</returns>
    public T WithScriptsFromDirectory(string folder)
    {
        InitBuilder.WithScriptsFromFileSystem(Path.Combine(folder, "Scripts", "Init"));
        MigrationsBuilder.WithScriptsFromFileSystem(Path.Combine(folder, "Scripts", "Migrations"));

        return (T)this;
    }

    /// <summary>
    /// Configures the migration engine to load scripts embedded in an assembly
    /// </summary>
    /// <param name="assembly">The assembly containing embedded script resources</param>
    /// <returns>The migration engine instance for method chaining</returns>
    public T WithScriptsFromAssembly(Assembly assembly)
    {
        InitBuilder.WithScriptsEmbeddedInAssembly(assembly, x => x.Contains(".Scripts.Init."));
        MigrationsBuilder.WithScriptsEmbeddedInAssembly(assembly, x => x.Contains(".Scripts.Migrations."));

        return (T)this;
    }

    /// <summary>
    /// Adds a script variable that will be available to both initialization and migration scripts
    /// </summary>
    /// <param name="name">The variable name</param>
    /// <param name="value">The variable value</param>
    /// <returns>The migration engine instance for method chaining</returns>
    public T WithVariable(string name, string value)
    {
        InitBuilder.WithVariable(name, value);
        MigrationsBuilder.WithVariable(name, value);

        return (T)this;
    }

    /// <summary>
    /// Adds multiple script variables that will be available to both initialization and migration scripts
    /// </summary>
    /// <param name="variables">A dictionary of variable names and values</param>
    /// <returns>The migration engine instance for method chaining</returns>
    public T WithVariables(IReadOnlyDictionary<string, string> variables)
    {
        var vars = variables.ToDictionary();
        InitBuilder.WithVariables(vars);
        MigrationsBuilder.WithVariables(vars);

        return (T)this;
    }

    /// <summary>
    /// Executes the database migration by running initialization scripts first, followed by migration scripts
    /// </summary>
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
