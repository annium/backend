using SysConsole = System.Console;
using System;
using System.IO;
using System.Threading;

namespace Annium.Logging.Console.Tests
{
    internal class ConsoleCapture : IDisposable
    {
        private static readonly object locker = new object();

        public static ConsoleCapture Start() => new ConsoleCapture();

        public string Output => writer.ToString();

        private TextWriter stdout;

        private StringWriter writer = new StringWriter();

        private ConsoleCapture()
        {
            Monitor.Enter(locker);
            stdout = SysConsole.Out;
            SysConsole.SetOut(writer);
        }

        public void Dispose()
        {
            SysConsole.SetOut(stdout);
            writer.Dispose();
            Monitor.Exit(locker);
        }
    }
}