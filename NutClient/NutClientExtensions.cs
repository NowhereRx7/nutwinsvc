using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace NutClient;

public static class NutClientExtensions
{
    //TODO: If I break up the client levels, this could be moved into a child class.
    public static async Task<string> GetAsync(this NutClient client, GetCommand command, string? arg, CancellationToken cancellationToken)
    {
        switch (command)
        {
            case GetCommand.DESC:
            case GetCommand.NUMLOGINS:
            case GetCommand.UPSDESC:
                CommandResult result = await client.ExecuteCommandAsync(Command.GET, [command.ToString()], cancellationToken);
                if (result.Error != Error.None)
                    throw new NutException(result);
                if (result.Data == null)
                    throw new NutException("Command executed but did not return a result!");
                string[] a = (result.Data as String)?.Split(" ", 2) ?? [];
                if (a[0] == command.ToString())
                {
                    return a[1]?.Trim('"') ?? string.Empty;
                }
                else
                    throw new NutException("Command executed but returned and invalid result!");
            default:
                throw new NotImplementedException();

        }
    }
}
