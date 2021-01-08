﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    [Description("Restore database")]
    public class RestoreCommand : Command<RestoreCommand.Settings>
    {
        public class Settings : BaseSettings
        {
            [CommandOption("-b|--backup-file <FilePath>")]
            [Description("Backup file name to restore database")]
            public string FilePath { get; set; }

            public string WithOptions()
            {
                var dataPath = GetServerProperty("InstanceDefaultDataPath");
                var logPath = GetServerProperty("InstanceDefaultLogPath");
                var fileTypes = new
                {
                    Data = "D",
                    Log = "L",
                    FileStream = "S"
                };

                var withOptions = new List<string> { "NOUNLOAD", "REPLACE", "STATS = 10" };

                foreach (var file in GetFileList())
                {
                    if (string.Equals(file.Type, fileTypes.Data, StringComparison.OrdinalIgnoreCase))
                        withOptions.Add($"MOVE '{file.LogicalName}' TO '{dataPath}{file.LogicalName}.MDF'");
                    else if (string.Equals(file.Type, fileTypes.Log, StringComparison.OrdinalIgnoreCase))
                        withOptions.Add($"MOVE '{file.LogicalName}' TO '{logPath}{file.LogicalName}.LDF'");
                    else if (string.Equals(file.Type, fileTypes.FileStream, StringComparison.OrdinalIgnoreCase)) 
                        withOptions.Add($"MOVE '{file.LogicalName}' TO '{dataPath}{Database}'");
                }

                return string.Join(", ", withOptions.ToArray());
            }

            private string GetServerProperty(string propertyName)
            {
                var sql = $@"SELECT SERVERPROPERTY('{propertyName}');";
                return this.ExecuteScalar<string>(sql);
            }

            private IEnumerable<(string LogicalName, string Type)> GetFileList()
            {
                var sql = $@"RESTORE FILELISTONLY FROM DISK = '{FilePath}';";

                return this.ExecuteReader(sql).Select(dr => (
                    dr.GetString(dr.GetOrdinal("LogicalName"))
                    , dr.GetString(dr.GetOrdinal("Type"))
                ));
            }
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
            Logger.Information($"Restoring database {settings.Database} from backup file {settings.FilePath}.");

            // if database doesn't exist, report error
            
            var masterSettings = settings.ForMaster();

            var sql = $@"
                IF EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '{settings.Database}')
	                ALTER DATABASE [{settings.Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [{settings.Database}] FROM DISK = '{settings.FilePath}'
                WITH {settings.WithOptions()};
                ALTER DATABASE [{settings.Database}] SET MULTI_USER;
            ";

            if (settings.Verbose)
                Logger.Information(sql);

            masterSettings.ExecuteNonQuery(sql, true);
            Logger.Information($"Database {settings.Database} restore complete.");

            return 0;
        }
    }
}