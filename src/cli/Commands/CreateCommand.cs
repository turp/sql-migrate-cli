using Spectre.Console.Cli;
using System.ComponentModel;

namespace Sql.Migrate.Cli.Commands;

[Description("Create database (if it doesn't already exist)")]
public class CreateCommand : Command<BaseSettings>
{
	public override int Execute(CommandContext context, BaseSettings settings)
	{
		Logger.Information($"Creating database {settings.Database} if it does not exist.");

		var sql = $@"
                IF EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '{settings.Database}')
                BEGIN
	                SELECT 'DATABASE {settings.Database} ALREADY EXISTS';
	                RETURN;
                END
                IF NOT EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '{settings.Database}')
                BEGIN
	                CREATE DATABASE [{settings.Database}];
	                IF EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '{settings.Database}')
		                SELECT 'DATABASE {settings.Database} CREATED';
                END
            ";

		var result = settings.ForMaster().ExecuteScalar<string>(sql);
		Logger.Information(result);
		return 0;
	}
}
