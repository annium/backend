using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.CommandLine
{
    public static class Cli
    {
        public static bool Confirm(string label, bool? defaultValue = null)
        {
            var y = defaultValue.HasValue && defaultValue.Value ? 'Y' : 'y';
            var n = defaultValue.HasValue && !defaultValue.Value ? 'N' : 'n';

            Console.WriteLine($"{label} ({y}/{n})");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Y)
                    return true;
                if (key.Key == ConsoleKey.N)
                    return false;
                if (defaultValue.HasValue && key.Key == ConsoleKey.Enter)
                    return defaultValue.Value;
            }
        }

        public static string Prompt(string label)
        {
            Console.Write(label);

            return Console.ReadLine();
        }

        public static string ReadSecure(string label)
        {
            Console.Write(label);
            var result = new Stack<char>();
            var pos = Console.CursorLeft;
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (result.Count > 0)
                        {
                            result.Pop();
                            Console.CursorLeft = --pos;
                            Console.Write(' ');
                            Console.CursorLeft = pos;
                        }
                        break;
                    case ConsoleKey.Enter:
                        break;
                    default:
                        result.Push(key.KeyChar);
                        Console.Write('*');
                        pos++;
                        break;
                }
            }
            // until enter is pressed
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();

            return string.Join(string.Empty, result.Reverse());
        }
    }
}