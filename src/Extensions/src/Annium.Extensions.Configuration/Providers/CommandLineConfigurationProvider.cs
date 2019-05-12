using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Annium.Extensions.Primitives;

namespace Annium.Extensions.Configuration
{
    internal class CommandLineConfigurationProvider : IConfigurationProvider
    {
        private const string separator = "|";

        private readonly string[] args;

        private Dictionary<string[], string> data;

        private Stack<string> context;

        private string[] path => context.Reverse().ToArray();

        public CommandLineConfigurationProvider(string[] args)
        {
            this.args = args;
        }

        public IReadOnlyDictionary<string[], string> Read()
        {
            data = new Dictionary<string[], string>();
            context = new Stack<string>();

            var flags = new List<string>();
            var options = new Dictionary<string, string>();
            var multiOptions = new Dictionary<string, List<string>>();

            for (var i = 0; i < args.Length; i++)
            {
                var value = args[i];
                if (IsPosition(value))
                    continue;

                var name = ParseName(value);
                var next = i < args.Length - 1 ? args[i + 1] : null;

                if (IsOption(value, next))
                {
                    if (multiOptions.ContainsKey(name))
                        multiOptions[name].Add(next);
                    else if (options.ContainsKey(name))
                    {
                        multiOptions[name] = new List<string>() { options[name], next };
                        options.Remove(name);
                    }
                    else
                        options[name] = next;
                    i++;
                }

                else if (IsFlag(value, next))
                    if (flags.Contains(name))
                        throw new Exception($"Same flag '{value}' is used twice");
                    else
                        flags.Add(name);

                else
                    throw new Exception($"Can't process value '{value}', followed by '{next}'");
            }

            foreach (var name in flags)
                data[name.Split(separator)] = true.ToString();

            foreach (var(name, value) in options)
                data[name.Split(separator)] = value;

            foreach (var(name, values) in multiOptions)
            {
                var path = name.Split(separator);
                for (var i = 0; i < values.Count; i++)
                    data[path.Append(i.ToString()).ToArray()] = values[i];
            }

            return data;
        }

        private bool IsPosition(string value) =>
            !IsOptionLike(value);

        private bool IsOption(string value, string next) =>
            IsOptionLike(value) && next != null && !IsOptionLike(next);

        private bool IsFlag(string value, string next) =>
            IsOptionLike(value) && (next == null || IsOptionLike(next));

        private bool IsOptionLike(string value) =>
            value.StartsWith('-');

        private string ParseName(string value) => string.Join(separator,
            Regex.Replace(value.Trim(), @"^-+", string.Empty)
            .Split('.')
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.PascalCase())
        );
    }

    public static class CommandLineConfigurationProviderExtensions
    {
        public static IConfigurationBuilder AddCommandLineArgs(this IConfigurationBuilder builder)
        {
            return AddCommandLineArgs(builder, Environment.GetCommandLineArgs().Skip(1).ToArray());
        }

        public static IConfigurationBuilder AddCommandLineArgs(this IConfigurationBuilder builder, string[] args)
        {
            var configuration = new CommandLineConfigurationProvider(args).Read();

            return builder.Add(configuration);
        }
    }
}