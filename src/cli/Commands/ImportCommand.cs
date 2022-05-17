using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    [Description("Import (deploys) procedures, functions, views and triggers from given server and database")]
    public class ImportCommand : Command<ImportCommand.Settings>
    {
        public sealed class Settings : BaseSettings
        {
            [CommandOption("-f|--folder <Folder>")]
            [Description("Path for script files")]
            public string Folder { get; set; }
        }

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            // Should be in base class
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            // Should be in base class
            if (settings is null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            return string.IsNullOrEmpty(settings.Folder) 
                ? ValidationResult.Error("Script folder must be specified") 
                : base.Validate(context, settings);
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            Logger.Information($"Importing to database {settings.Server}.{settings.Database}");
            Logger.Information($"Schema {settings.Schema}");

            var filePath = Path.GetFullPath(settings.Folder);

            Logger.Information($"Executing {Path.GetFileName(filePath)}");

            var batches = filePath.GetSqlBatches();
            settings.ExecuteNonQuery(batches, settings.Verbose);

            return 0;
        }
	}
}