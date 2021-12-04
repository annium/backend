using System;
using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

internal interface IHelpBuilder
{
    string BuildHelp(string command, string description, IReadOnlyCollection<CommandBase> commands);

    string BuildHelp(string command, string description, params Type[] configurationTypes);
}