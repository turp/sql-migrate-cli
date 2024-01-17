using Spectre.Console.Cli;

namespace Sql.Migrate.Cli.Commands;

public class CreateSchemaCommand : Command<CreateSchemaCommand.Settings>
{
	public sealed class Settings : BaseSettings
	{
	}

	public override int Execute(CommandContext context, Settings settings)
	{
		var sql = $@"
                IF NOT EXISTS (SELECT * FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[dbo].[SchemaVersion]') AND [type] IN (N'U'))
                BEGIN
	                CREATE TABLE [dbo].[SchemaVersion] (
		                [Version] [int] NOT NULL,
		                [Script] nvarchar(250) NOT NULL,
		                [ScriptRunDate] datetime NOT NULL,
		                [SchemaName] VARCHAR(25) NOT NULL CONSTRAINT DF_SchemaVersion_SchemaName DEFAULT 'dbo',
		                CONSTRAINT [PK_Schema_Version] PRIMARY KEY CLUSTERED (SchemaName ASC, [Version] ASC) )
                END;

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SchemaVersion' AND COLUMN_NAME = 'SchemaName')
                BEGIN
	                ALTER TABLE [dbo].[SchemaVersion]
	                ADD [SchemaName] VARCHAR(25) NOT NULL CONSTRAINT DF_SchemaVersion_SchemaName DEFAULT 'dbo'
                END;

                IF EXISTS (SELECT * FROM [sys].[indexes] WHERE [object_id] = OBJECT_ID(N'[dbo].[SchemaVersion]') AND [name] = N'PK_Version')
                BEGIN
	                ALTER TABLE [dbo].[SchemaVersion] DROP CONSTRAINT [PK_Version]
                END;

                IF NOT EXISTS (SELECT * FROM [sys].[indexes] WHERE [object_id] = OBJECT_ID(N'[dbo].[SchemaVersion]') AND [name] = N'PK_Schema_Version')
                BEGIN
	                ALTER TABLE [dbo].[SchemaVersion] ADD CONSTRAINT [PK_Schema_Version] PRIMARY KEY CLUSTERED
		                ([SchemaName] ASC, [Version] ASC)
                END;
            ";

		settings.ExecuteNonQuery(sql);
		return 0;
	}
}