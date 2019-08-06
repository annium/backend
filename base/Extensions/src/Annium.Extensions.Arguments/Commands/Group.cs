using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Arguments
{
    public abstract class Group : CommandBase
    {
        private List<Type> commands = new List<Type>();

        public Group Add<T>()
        where T : CommandBase
        {
            commands.Add(typeof(T));

            return this;
        }

        public override void Process(string command, string[] args, CancellationToken token)
        {
            var commands = this.commands.Select(Root.Provider.GetRequiredService).OfType<CommandBase>().ToArray();
            CommandBase cmd;

            // if any args - try to find command by id and execute it
            if (args.Length > 0)
            {
                // find command to execute by id
                var id = args[0];
                cmd = commands.FirstOrDefault(e => e.Id == id);
                if (cmd != null)
                {
                    cmd.Root = Root;
                    cmd.Process($"{command} {id}".Trim(), args.Skip(1).ToArray(), token);
                    return;
                }
            }

            // if no command found, or no args - try to find default command and execute it
            cmd = commands.FirstOrDefault(e => e.Id == string.Empty);
            if (cmd != null)
            {
                cmd.Root = Root;
                cmd.Process(command, args, token);
                return;
            }

            if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(Root.HelpBuilder.BuildHelp(command, Description, commands));
                return;
            }
        }
    }
}