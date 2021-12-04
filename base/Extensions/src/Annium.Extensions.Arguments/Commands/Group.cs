using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments;

public abstract class Group : CommandBase
{
    private readonly List<Type> _commands = new();

    public Group Add<T>()
        where T : CommandBase
    {
        _commands.Add(typeof(T));

        return this;
    }

    public override void Process(string command, string[] args, CancellationToken ct)
    {
        var root = Root!;
        var commands = _commands.Select(root.Provider.Resolve).OfType<CommandBase>().ToArray();
        CommandBase? cmd;

        // if any args - try to find command by id and execute it
        if (args.Length > 0)
        {
            // find command to execute by id
            var id = args[0];
            cmd = commands.FirstOrDefault(e => e.Id == id);
            if (cmd != null)
            {
                cmd.SetRoot(root);
                cmd.Process($"{command} {id}".Trim(), args.Skip(1).ToArray(), ct);
                return;
            }
        }

        // if no command found, or no args - try to find default command and execute it
        cmd = commands.FirstOrDefault(e => e.Id == string.Empty);
        if (cmd != null)
        {
            cmd.SetRoot(root);
            cmd.Process(command, args, ct);
            return;
        }

        if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description, commands));
        }
    }
}