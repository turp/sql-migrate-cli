using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    [Description("Extracts procedures, functions, views and triggers from given server and database")]
    public class ExtractCommand : Command<ExtractCommand.Settings>
    {
        public sealed class Settings : BaseSettings
        {
            [CommandOption("-o|--out <Folder>")]
            [Description("Path for writing script files")]
            public string Folder { get; set; }

            [CommandOption("-f|--force")]
            [Description("Overwrite existing script files")]
            public bool Force { get; set; } = true;
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
                ? ValidationResult.Error("Output folder must be specified") 
                : base.Validate(context, settings);
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            Logger.Information($"Extracting database {settings.Server}.{settings.Database}");
            Logger.Information($"Schema {settings.Schema}");
            
            var list = GetDatabaseObjects(settings).ToList();

            if (!list.Any())
            {
                Logger.Error($"No objects found in database {settings.Server}: {settings.Database}");
                return -1;
            }

            foreach (var item in list)
                CreateScript(settings, item);

            return 0;
        }

        private static IEnumerable<SqlObject> GetDatabaseObjects(Settings settings)
        {
            const string sql = @"
                SELECT '[' + s.name + '].[' + o.name + ']' AS [name]
	                ,CASE type WHEN 'P' THEN 'Procedure' WHEN 'V' THEN 'View' WHEN 'TR' THEN 'Trigger' ELSE 'Function' END as [Type]
                FROM sys.objects o
	                JOIN sys.schemas s ON o.schema_id = s.schema_id
                WHERE type in ('P', 'V', 'FN', 'IF', 'TF', 'FS', 'FT', 'TR')
                AND o.name NOT LIKE 'dt_%'
                AND o.name NOT IN (
	                'fn_diagramobjects',
	                'sp_alterdiagram',
	                'sp_creatediagram',
	                'sp_dropdiagram',
	                'sp_helpdiagramdefinition',
	                'sp_helpdiagrams',
	                'sp_renamediagram',
	                'sp_upgraddiagrams')
                ORDER BY s.name, o.name;
            ";

            return settings.ExecuteReader(sql).Select(record => new SqlObject()
            {
                Name = record.GetString(0),
                Type = record.GetString(1)
            });
        }

        private class SqlObject
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }

        private static void CreateScript(Settings settings, SqlObject sqlObject)
        {
            var path = Path.Combine(settings.Folder, sqlObject.Type + "s");
            var fileName = Path.Combine(path,
                sqlObject.Name
                    .Replace("[dbo].", string.Empty)
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                + ".sql"
            );

            if (File.Exists(fileName) && !settings.Force)
            {
                Logger.Information($"SKIPPED: {sqlObject.Name}");
                return;
            }

            CreateFolder(path);

            try
            {
                using var sw = File.CreateText(fileName);
                Logger.Information($"{sqlObject.Name}");
                CreateFileHeader(sqlObject, sw);

                foreach (var record in settings.ExecuteReader($"sp_helptext '{sqlObject.Name}'"))
                {
                    sw.Write(record.GetString(0));
                }
                
                CreateFileFooter(sqlObject, sw);
            }
            catch
            {
                Logger.Error($"ERROR: {sqlObject.Name} - unable to execute sp_helptext");
            }
        }

        private static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void CreateFileFooter(SqlObject o, StreamWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine("GO");
            sw.WriteLine();
            sw.WriteLine("IF OBJECT_ID('{0}') IS NOT NULL", o.Name);
            sw.WriteLine("\tPRINT '<<< CREATED {0} {1} >>>'", o.Type.ToUpper(), o.Name);
            sw.WriteLine("ELSE");
            sw.WriteLine("\tPRINT '<<< FAILED CREATING {0} {1} >>>'", o.Type.ToUpper(), o.Name);
            sw.WriteLine("GO");
            sw.WriteLine();
        }

        private static void CreateFileHeader(SqlObject o, StreamWriter sw)
        {
            sw.WriteLine("IF OBJECT_ID('{0}') IS NOT NULL", o.Name);
            sw.WriteLine("\tDROP {0} {1}", o.Type.ToUpper(), o.Name);
            sw.WriteLine("GO");
        }
	}
}