// using System;
// using System.Linq;
// using System.Reflection;

// namespace Annium.Core.Reflection
// {
//     public static class ResolveGenericArgumentsByImplentationExtensionBackup
//     {
//         private static Type[] backup(this Type type, Type target)
//         {
//             if (type is null)
//                 throw new ArgumentNullException(nameof(type));

//             if (target is null)
//                 throw new ArgumentNullException(nameof(target));

//             if (type.IsGenericParameter)
//             {
//                 var attrs = type.GenericParameterAttributes;

//                 // if reference type required, but target is not class
//                 if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && !target.IsClass)
//                     return null;

//                 // if not nullable value type required, but target is not not-nullable value type
//                 if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && !target.IsNotNullableValueType())
//                     return null;

//                 var meetsConstraints = type.GetGenericParameterConstraints()
//                     .All(constraint => target.GetTargetImplementation(constraint) != null);

//                 return meetsConstraints ? new [] { target } : null;
//             }

//             // if type is not generic - return empty array, meaning successful resolution
//             if (!type.IsGenericType)
//                 return Type.EmptyTypes;

//             // if type is defined or target is not generic - no need for resolution, just return type's generic args
//             if (!type.ContainsGenericParameters || !target.IsGenericType)
//                 return type.GetGenericArguments();

//             // if same generic - return target's arguments
//             if (type.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
//                 return target.GetGenericArguments();

//             // type is generic with parameters, target is generic without parameters

//             if (target.IsValueType)
//                 return null;

//             if (target.IsClass)
//             {
//                 var baseType = type.BaseType;

//                 // if no base type or it's not generic - resolution fails, cause types' generic definitions are different
//                 if (baseType is null || !baseType.IsGenericType)
//                     return null;

//                 if (baseType.GetGenericTypeDefinition() != target.GetGenericTypeDefinition())
//                     return resolveBase();

//                 // base type is generic class type with same base definition, as target
//                 return buildArgs(baseType);
//             }

//             if (target.IsInterface)
//             {
//                 // find interface, that is implementation of target's generic definition
//                 var targetBase = target.GetGenericTypeDefinition();
//                 var implementation = type.GetOwnInterfaces()
//                     .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

//                 if (implementation != null)
//                     // implementation is generic interface type with same base definition, as target
//                     return buildArgs(implementation);

//                 if (type.BaseType is null)
//                     return null;

//                 return resolveBase();
//             }

//             // otherwise - not implemented or don't know how to resolve
//             throw new NotImplementedException($"Can't resolve {type.Name} implementation of {target.Name}");

//             Type[] resolveBase()
//             {
//                 var unboundBaseType = type.GetUnboundBaseType();
//                 var baseArgs = unboundBaseType.ResolveGenericArgumentsByImplentation(target);
//                 if (baseArgs is null)
//                     return null;

//                 if (!type.BaseType.GetGenericTypeDefinition().TryMakeGenericType(out var baseImplementation, baseArgs))
//                     return null;

//                 return type.ResolveGenericArgumentsByImplentation(baseImplementation);
//             }

//             Type[] buildArgs(Type sourceType)
//             {
//                 var args = type.GetGenericArguments();

//                 fillArgs(args, sourceType, target);

//                 var unresolvedArgs = args.Count(a => a.IsGenericTypeParameter);
//                 if (unresolvedArgs == 0 || unresolvedArgs == args.Length)
//                     return args;

//                 var originalArgs = type.GetGenericArguments();

//                 while (true)
//                 {
//                     for (var i = 0; i < args.Length; i++)
//                     {
//                         var arg = args[i];
//                         if (arg.IsGenericTypeParameter)
//                             continue;

//                         foreach (var constraint in originalArgs[i].GetGenericParameterConstraints())
//                             fillArgs(args, constraint, arg);
//                     }

//                     var currentlyUnresolved = args.Count(a => a.IsGenericTypeParameter);
//                     if (currentlyUnresolved == 0 || currentlyUnresolved == unresolvedArgs)
//                         break;

//                     unresolvedArgs = currentlyUnresolved;
//                 }

//                 return args;
//             }

//             void fillArgs(Type[] args, Type sourceType, Type targetType)
//             {
//                 targetType = targetType.GetTargetImplementation(sourceType);
//                 if (targetType is null || !targetType.IsGenericType)
//                     return;

//                 var sourceArgs = sourceType.GetGenericArguments();
//                 var targetArgs = targetType.GetGenericArguments();

//                 for (var i = 0; i < sourceArgs.Length; i++)
//                 {
//                     if (sourceArgs[i].IsGenericParameter)
//                         args[sourceArgs[i].GenericParameterPosition] = targetArgs[i];
//                     else if (sourceArgs[i].ContainsGenericParameters)
//                         fillArgs(args, sourceArgs[i], targetArgs[i]);
//                 }
//             }
//         }
//     }
// }