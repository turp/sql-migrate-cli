using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Sql.Migrate.Cli.Commands;

[Description("Displays current schema version for the given server and database")]
public class SchemaCommand : Command<BaseSettings>
{
	public override int Execute(CommandContext context, BaseSettings settings)
	{
		CreateTableIfNotExists(settings);

		var sql = $@"
                IF NOT EXISTS (SELECT * FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[dbo].[SchemaVersion]') AND [type] IN (N'U'))
	                SELECT 0 AS [Version]
                ELSE
	                SELECT TOP 1 [Version] FROM [dbo].[SchemaVersion] WHERE [SchemaName] = '{settings.Schema}' ORDER BY [Version] DESC
            ";

		var result = settings.ExecuteScalar<int>(sql);
		AnsiConsole.WriteLine($"Current Schema Version: {result}");
		return result;
	}

	private void CreateTableIfNotExists(BaseSettings settings)
	{
		var sql = $@"
                IF EXISTS (SELECT * FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[dbo].[SchemaVersion]') AND [type] IN (N'U'))
                    RETURN;

                CREATE TABLE [dbo].[SchemaVersion] (
	                [Version] [int] NOT NULL,
	                [Script] nvarchar(250) NOT NULL,
	                [ScriptRunDate] datetime NOT NULL,
	                [SchemaName] VARCHAR(25) NOT NULL CONSTRAINT DF_SchemaVersion_SchemaName DEFAULT 'dbo',
	                CONSTRAINT [PK_Schema_Version] PRIMARY KEY CLUSTERED (SchemaName ASC, [Version] ASC) 
                )
            ";

		settings.ExecuteNonQuery(sql);
	}
}
