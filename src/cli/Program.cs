using Spectre.Console.Cli;
using Sql.Migrate.Cli.Commands;

namespace Sql.Migrate.Cli;

public class Program
{
	public static int Main(string[] args)
	{
		var app = new CommandApp();

		app.Configure(config =>
		{
			config.AddCommand<CreateCommand>("create");
			config.AddCommand<DropCommand>("drop");
			config.AddCommand<BackupCommand>("backup");
			config.AddCommand<RestoreCommand>("restore");
			config.AddCommand<SchemaCommand>("schema");
			config.AddCommand<ExportCommand>("export");
			config.AddCommand<ImportCommand>("import");
			config.AddCommand<MigrateCommand>("migrate");
		});

		return app.Run(args);
	}
}

// public static class SettingsDumper
// {
//     public static void Dump(CommandSettings settings)
//     {
//         var table = new Table().RoundedBorder();
//         table.AddColumn("[grey]Name[/]");
//         table.AddColumn("[grey]Value[/]");
//
//         var properties = settings.GetType().GetProperties();
//         foreach (var property in properties)
//         {
//             var value = property.GetValue(settings)
//                 ?.ToString()
//                 ?.Replace("[", "[[");
//
//             table.AddRow(
//                 property.Name,
//                 value ?? "[grey]null[/]");
//         }
//
//         AnsiConsole.Render(table);
//     }
// }

