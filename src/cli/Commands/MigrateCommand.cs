using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    [Description("Migrate database schema")]
    public class MigrateCommand : Command<MigrateCommand.Settings>
    {
        public class Settings : BaseSettings
        {
            [CommandOption("-p|--path <FilePath>")]
            [Description("Path to migration scripts")]
            public string FilePath { get; set; }
        }

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            // Should be in base class
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Should be in base class
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return string.IsNullOrEmpty(settings.FilePath)
                ? ValidationResult.Error("Path to migration scripts must be specified")
                : base.Validate(context, settings);
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var currentVersion = new SchemaCommand().Execute(context, settings);

            var scripts = MigrationScript.GetScripts(settings.FilePath, settings.Schema, currentVersion).ToList();

            if (!scripts.Any())
            {
                Logger.Warning("Database migration: schema is up to date");
                return currentVersion;
            }

            Logger.Information($"Migrating database from version {currentVersion}");

            foreach (var script in scripts)
            {
                ExecuteMigrationScript(settings, script);
                Logger.Information($"{script.FileName} applied");
            }

            currentVersion = new SchemaCommand().Execute(context, settings);
            Logger.Information($"Database migration completed and is now at version {currentVersion}.");
            return currentVersion;
        }

        private void ExecuteMigrationScript(BaseSettings settings, MigrationScript script)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));

            var batches = script.FilePath.GetSqlBatches();
            batches.Add(script.InsertVersion());
            settings.ExecuteNonQuery(batches);
        }

        private class MigrationScript
        {
            public string FilePath { get; private set; }
            private string Schema { get; set; }

            public string FileName => string.IsNullOrWhiteSpace(FilePath) ? null : Path.GetFileName(FilePath);

            private int? Version
            {
                get
                {
                    var filename = FileName;
                    if (string.IsNullOrWhiteSpace(filename))
                        return null;

                    var parts = filename.Split('_');
                    if (parts.Length < 2)
                        return null;

                    if (int.TryParse(parts[0], out var version))
                        return version;

                    return null;
                }
            }

            public string InsertVersion()
            {
                if (Version == null)
                    return string.Empty;

                if (string.IsNullOrWhiteSpace(Schema))
                    Schema = "dbo";

                var sql = $@"
                    INSERT INTO [dbo].[SchemaVersion] ([Version], [Script], [SchemaName], [ScriptRunDate])
                    VALUES({Version}, '{FileName}', '{Schema}', GETDATE());";

                return sql;
            }

            public static IEnumerable<MigrationScript> GetScripts(
                string folder,
                string schema,
                int currentVersion)
            {
                var fullPath = Path.GetFullPath(folder);

                if (!Directory.Exists(fullPath))
                    throw new Exception($"{fullPath} does not exist");

                var files = Directory.GetFiles(fullPath, "*.sql", SearchOption.TopDirectoryOnly);

                var scripts = files
                    .Select(f => new MigrationScript
                    {
                        FilePath = f,
                        Schema = schema
                    })
                    .Where(s => s.Version > currentVersion)
                    .OrderBy(s => s.Version)
                    .ThenBy(s => s.FileName)
                    .ToList();

                return scripts;
            }
        }

    }
}