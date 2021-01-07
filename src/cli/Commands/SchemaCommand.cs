using System;
using System.Collections.Generic;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    [Description("Displays current schema version for the given server and database")]
    public class SchemaCommand : Command<SchemaCommand.Settings>
    {
        public sealed class Settings : BaseSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var sql = $@"
                IF NOT EXISTS (SELECT * FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[dbo].[SchemaVersion]') AND [type] IN (N'U'))
	                SELECT 0 AS [Version];
                ELSE
	                SELECT TOP 1 [Version] FROM [dbo].[SchemaVersion] WHERE [SchemaName] = '{settings.Schema}' ORDER BY [Version] DESC;
            ";

            var result = settings.ExecuteScalar<int>(sql);
            AnsiConsole.WriteLine($"Current Schema Version: {result}");
            return result;
        }
    }
}
