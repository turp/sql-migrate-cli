using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Spectre.Console;

namespace Db.Deploy.Cli.Commands
{
    public static class Extensions
    {
        public static string ToOsCompatiblePath(this string path)
        {
            if (path == null)
                return null;

            if (path.Contains('/') && Path.DirectorySeparatorChar != '/')
                return path.Replace('/', Path.DirectorySeparatorChar);
            if (path.Contains('\\') && Path.DirectorySeparatorChar != '\\')
                return path.Replace('\\', Path.DirectorySeparatorChar);

            return path;
        }

        public static void ExecuteNonQuery(this BaseSettings settings, string sql, bool verbose = false)
        {
            using var connection = CreateConnection(settings, verbose);
            using var command = new SqlCommand(sql, connection) {CommandTimeout = 600};
            command.ExecuteNonQuery();
        }

        public static IEnumerable<IDataRecord> ExecuteReader(this BaseSettings settings, string sql, bool verbose = false)
        {
            using var connection = CreateConnection(settings, verbose);
            using var command = new SqlCommand(sql, connection) {CommandTimeout = 600};
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                yield return reader;
            }

            reader.Close();
        }
 
        public static T ExecuteScalar<T>(this BaseSettings settings, string sql, bool verbose = false)
        {
            using var connection = CreateConnection(settings, verbose);
            using var command = new SqlCommand(sql, connection) {CommandTimeout = 600};

            var val = command.ExecuteScalar();
            if (val == null) return default(T);
            return (T)Convert.ChangeType(val, typeof(T));
        }


        private static SqlConnection CreateConnection(BaseSettings settings, bool verbose)
        {
            var connection = new SqlConnection(settings.GetConnectionString());
            if (verbose)
            {
                connection.FireInfoMessageEventOnUserErrors = true;
                connection.InfoMessage += ConnectionInfoMessageHandler;
            }
            connection.Open();

            return connection;
        }

        private static void ConnectionInfoMessageHandler(object sender, SqlInfoMessageEventArgs e)
        {
            foreach (SqlError error in e.Errors)
            {
                if (error.Class == 0)
                    AnsiConsole.MarkupLine($"[yellow]{error.Message}[/]");
                else
                    AnsiConsole.MarkupLine($"[red]{error.Message}[/]");
            }
        }
    }
}