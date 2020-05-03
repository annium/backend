using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Annium.Core.Runtime.Types
{
    internal class TypesCollector
    {
        private const string LibraryFileExtension = ".dll";

        private readonly Assembly _assembly;

        public TypesCollector(
            Assembly assembly
        )
        {
            _assembly = assembly;
        }

        public Type[] CollectTypes()
        {
            var core = typeof(object).Assembly.GetName();
            var assemblyNames = DependencyContext.Load(_assembly).CompileLibraries
                .Select(x => new AssemblyName(x.Name))
                .Prepend(core)
                .ToArray();

            var path = _assembly.Location;
            if (!File.Exists(path))
                return assemblyNames.SelectMany(GeneralAssemblyLoadTypes).ToArray();

            var directory = Path.GetDirectoryName(path)!;

            return assemblyNames.SelectMany(LocatedAssemblyLoadTypes(directory)).ToArray();
        }

        private Type[] GeneralAssemblyLoadTypes(AssemblyName name)
        {
            try
            {
                return Assembly.Load(name).GetTypes();
            }
            catch (Exception e) when (
                e is FileNotFoundException ||
                e is FileLoadException
            )
            {
                return Type.EmptyTypes;
            }
            catch
            {
                return Type.EmptyTypes;
            }
        }

        private Func<AssemblyName, Type[]> LocatedAssemblyLoadTypes(string directory) => name =>
        {
            var assemblyPath = Path.Combine(directory, $"{name.Name}{LibraryFileExtension}");
            try
            {
                if (File.Exists(assemblyPath))
                    return Assembly.LoadFrom(assemblyPath).GetTypes();

                return Assembly.Load(name).GetTypes();
            }
            catch (Exception e) when (
                e is FileNotFoundException ||
                e is FileLoadException
            )
            {
                return Type.EmptyTypes;
            }
            catch
            {
                return Type.EmptyTypes;
            }
        };
    }
}