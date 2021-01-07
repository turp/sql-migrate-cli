using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    public class BaseSettings : CommandSettings
    {
        [CommandOption("-s|--server <SERVER>")]
        [Description("SQL Server name")]
        public string Server { get; set; }
        
        [CommandOption("-d|--database <DATABASE>")]
        [Description("Database name")]
        public string Database { get; set; }

        [CommandOption("-w|--owner <SCHEMA>")]
        [Description("Schema Owner. Defaults to dbo")]
        public string Schema { get; set; } = "dbo";

        public bool IntegratedSecurity => string.IsNullOrWhiteSpace(UserId);

        [CommandOption("-u|--user <USER>")]
        [Description("SQL Server login. If not provided, integrated security will be used")]
        public string UserId { get; set; }

        [CommandOption("-p|--password <PASSWORD>")]
        [Description("Password. Only required if --user is specified")]
        public string Password { get; set; }

        public string GetConnectionString()
        {
            IsSet(this);

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                InitialCatalog = Database,
                IntegratedSecurity = IntegratedSecurity
            };
        
            if (!string.IsNullOrWhiteSpace(UserId))
                builder.UserID = UserId;
            if (!string.IsNullOrWhiteSpace(Password))
                builder.Password = Password;
        
            return builder.ToString();
        }

        private static void IsSet(BaseSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.Server))
                throw new ArgumentException("Server is required.");
            if (string.IsNullOrWhiteSpace(settings.Database))
                throw new ArgumentException("Database is required.");
        }
    }

    public static class Extensions
    {
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
