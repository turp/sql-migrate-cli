using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
            Logger.Information($"Importing to Server {settings.Server}");
            Logger.Information($"Database {settings.Database}");
            Logger.Information($"Schema {settings.Schema}");

            var filePath = Path.GetFullPath(settings.Folder);
            var sql = GetSql(filePath, settings.Verbose);

            var batches = sql
                .Split('\n')
                .GetSqlBatches();

            settings.ExecuteNonQuery(batches, settings.Verbose);

            return 0;
        }

        private string GetSql(string filePath, bool verbose)
        {
            var sb = new StringBuilder();

            if (File.Exists(filePath))
            {
                foreach (var line in File.ReadAllLines(filePath))
                {
                    sb.AppendLine(line);
                }

                if (verbose)
                    Logger.Information($"Appending {Path.GetFileName(filePath)}");

                return sb.ToString();
            }

            if (!Directory.Exists(filePath)) return string.Empty;

            foreach (var file in Directory.GetFiles(filePath, "*.sql", SearchOption.AllDirectories))
            {
                foreach (var line in File.ReadAllLines(file))
                {
                    sb.AppendLine(line);
                }

                if (verbose)
                    Logger.Information($"Appending {Path.GetFileName(file)}");

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}