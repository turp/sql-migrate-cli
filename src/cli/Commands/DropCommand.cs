using Spectre.Console.Cli;
using System.ComponentModel;

namespace Sql.Migrate.Cli.Commands;

[Description("Drop database (if it exists)")]
public class DropCommand : Command<BaseSettings>
{
	public override int Execute(CommandContext context, BaseSettings settings)
	{
		Logger.Information($"Dropping database {settings.Database}");

		var sql = $@"
                IF NOT EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '{settings.Database}')
                BEGIN
	                SELECT 'DATABASE {settings.Database} DOES NOT EXIST';
	                RETURN;
                END

	            ALTER DATABASE [{settings.Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	            DROP DATABASE [{settings.Database}];

                IF NOT EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '{settings.Database}')
                    SELECT 'DATABASE {settings.Database} DROPPED SUCCESSFULLY'
                ELSE
                    SELECT 'FAILED TO DROP DATABASE {settings.Database}'
            ";

		var result = settings.ForMaster().ExecuteScalar<string>(sql);
		Logger.Information(result);
		return 0;
	}
}