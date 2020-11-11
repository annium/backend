using System;
using System.IO;
using System.Threading;
using SysConsole = System.Console;

namespace Annium.Logging.Console.Tests
{
    internal class ConsoleCapture : IDisposable
    {
        private static readonly object Locker = new object();

        public static ConsoleCapture Start() => new ConsoleCapture();

        public string Output => _writer.ToString();

        private TextWriter _stdout;

        private StringWriter _writer = new StringWriter();

        private ConsoleCapture()
        {
            Monitor.Enter(Locker);
            _stdout = SysConsole.Out;
            SysConsole.SetOut(_writer);
        }

        public void Dispose()
        {
            SysConsole.SetOut(_stdout);
            _writer.Dispose();
            Monitor.Exit(Locker);
        }
    }
}