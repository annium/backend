using System;
using System.Runtime.CompilerServices;
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
            [CallerMemberName] string member = ""
        ) where T : class
        {
            Log.Debug(() => $"{obj.GetType().Name}#{obj.GetId()}{(withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)} >> {getMessage()}", callerFilePath, member);
        }

        public static void Debug<T>(
            this T obj,
            bool withTrace = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        ) where T : class
        {
            Log.Debug(() => $"{obj.GetType().Name}#{obj.GetId()}{(withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)}", callerFilePath, member);
        }

        public static void Trace<T>(
            this T obj,
            Func<string> getMessage,
            bool withTrace = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        ) where T : class
        {
            Log.Trace(() => $"{obj.GetType().Name}#{obj.GetId()}{(withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)} >> {getMessage()}", callerFilePath, member);
        }

        public static void Trace<T>(
            this T obj,
            bool withTrace = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        ) where T : class
        {
            Log.Trace(() => $"{obj.GetType().Name}#{obj.GetId()}{(withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)}", callerFilePath, member);
        }
    }
}