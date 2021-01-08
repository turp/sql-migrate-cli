using System;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    [Description("Backup database to file")]
    public class BackupCommand : Command<BackupCommand.Settings>
    {
        public sealed class Settings : BaseSettings
        {
            [CommandOption("-b|--backup-file <FilePath>")]
            [Description("File name to store database backup")]
            public string FilePath { get ; set; }
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
                ? ValidationResult.Error("Backup filename must be specified")
                : base.Validate(context, settings);
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var backupFilePath = settings.FilePath.ToOsCompatiblePath();

            Logger.Information($"Backing up database {settings.Database} to backup file {backupFilePath}.");

            var sql = $@"
                BACKUP DATABASE [{settings.Database}]
                TO DISK = '{backupFilePath}'
                WITH FORMAT, INIT, COPY_ONLY, SKIP, REWIND, NOUNLOAD, STATS = 10, NAME = '{settings.Database} Full Backup';
            ";

            if (settings.Verbose)
            {
                Console.Write("Executing SQL:");
                Console.WriteLine(sql);
            }

            settings.ForMaster().ExecuteNonQuery(sql, true);
            return 0;
        }
    }
}