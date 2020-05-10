using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;

namespace Annium.Core.Runtime.Types
{
    internal class TypesCollector
    {
        private readonly DependencyContext _dependencyContext;
        private readonly AssemblyLoadContext _loadContext;

        public TypesCollector(
            Assembly assembly
        )
        {
            _dependencyContext = DependencyContext.Load(assembly);
            _loadContext = AssemblyLoadContext.GetLoadContext(assembly)!;
        }

        public Type[] CollectTypes()
        {
            var core = typeof(object).Assembly.GetName();
            var assemblyNames = _dependencyContext.CompileLibraries
                .Select(x => new AssemblyName(x.Name))
                .Prepend(core)
                .ToArray();

            return assemblyNames.SelectMany(CollectAssemblyTypes).ToArray();
        }

        private Type[] CollectAssemblyTypes(AssemblyName name)
        {
            try
            {
                var assembly = _loadContext.LoadFromAssemblyName(name);
                return assembly.GetTypes();
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
    }
 }