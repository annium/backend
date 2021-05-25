using System;
using System.Runtime.CompilerServices;
using Annium.Core.Primitives;
using Annium.Diagnostics.Debug;

namespace Annium.Core.Internal
{
    public static class LogExtensions
    {
        public static void Debug<T>(
            this T obj,
            Func<string> getMessage,
            bool withTrace = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) where T : class
        {
            Log.Debug(
                () =>
                    $"{obj.GetType().FriendlyName()}#{obj.GetId()}{(withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)} >> {getMessage()}",
                callerFilePath,
                member,
                line
            );
        }

        public static void Debug<T>(
            this T obj,
            bool withTrace = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) where T : class
        {
            Log.Debug(
                () =>
                    $"{obj.GetType().FriendlyName()}#{obj.GetId()}{(withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)}",
                callerFilePath,
                member,
                line
            );
        }

        public static void Trace<T>(
            this T obj,
            Func<string> getMessage,
            bool withTrace = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) where T : class
        {
            Log.Trace(
                () =>
                    $"{obj.GetType().FriendlyName()}#{obj.GetId()}{(withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)} >> {getMessage()}",
                callerFilePath,
                member,
                line
            );
        }

        public static void Trace<T>(
            this T obj,
            bool withTrace = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) where T : class
        {
            Log.Trace(
                () =>
                    $"{obj.GetType().FriendlyName()}#{obj.GetId()}{(withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)}",
                callerFilePath,
                member,
                line
            );
        }
    }
}