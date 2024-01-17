using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Sql.Migrate.Cli.Commands;

public static class Extensions
{
	public static IList<string> GetSqlBatches(this string filePath)
	{
		return File.ReadAllLines(filePath).GetSqlBatches();
	}

	public static IList<string> GetSqlBatches(this string[] lines)
	{
		var batches = new List<string>();
		var sb = new StringBuilder();

		foreach (var line in lines)
		{
			if (string.Equals("go", line.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				if (sb.Length > 0)
				{
					batches.Add(sb.ToString());
					sb.Clear();
				}

				continue;
			}

			sb.AppendLine(line);
		}

		if (sb.Length > 0)
			batches.Add(sb.ToString());

		return batches;
	}

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
		using var connection = CreateConnection(settings);
		using var command = new SqlCommand(sql, connection) { CommandTimeout = 600 };
		command.ExecuteNonQuery();
	}

	public static void ExecuteNonQuery(this BaseSettings settings, IEnumerable<string> batches, bool verbose = false)
	{
		if (!batches.Any())
		{
			Logger.Information("Nothing to execute.");
			return;
		}

		using var connection = CreateConnection(settings);
		var transaction = connection.BeginTransaction("ExecuteSqlBatches");

		try
		{
			using (var command = new SqlCommand())
			{
				command.Connection = connection;
				command.Transaction = transaction;
				command.CommandTimeout = 600;

				foreach (var sql in batches)
				{
					command.CommandText = sql;
					command.ExecuteNonQuery();
				}
			}

			transaction.Commit();
		}
		catch (Exception)
		{
			Logger.Error("Failed to execute SQL statements");

			try
			{
				transaction.Rollback();
			}
			catch (Exception x2)
			{
				Logger.Error("Transaction rollback failed.");
				Logger.Error($"Exception: {x2.GetType()}: {x2.Message}.");
			}

			throw;
		}
	}

	public static IEnumerable<IDataRecord> ExecuteReader(this BaseSettings settings, string sql)
	{
		using var connection = CreateConnection(settings);
		using var command = new SqlCommand(sql, connection) { CommandTimeout = 600 };
		using var reader = command.ExecuteReader();

		while (reader.Read())
		{
			yield return reader;
		}

		reader.Close();
	}

	public static T ExecuteScalar<T>(this BaseSettings settings, string sql)
	{
		using var connection = CreateConnection(settings);
		using var command = new SqlCommand(sql, connection) { CommandTimeout = 600 };

		var val = command.ExecuteScalar();
		if (val == null) return default(T);
		return (T)Convert.ChangeType(val, typeof(T));
	}

	private static SqlConnection CreateConnection(BaseSettings settings)
	{
		var connection = new SqlConnection(settings.GetConnectionString());
		if (settings.Verbose)
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
			AnsiConsole.MarkupLine(error.Class == 0 ? "[yellow]{0}[/]" : "[red]{0}[/]",
				error.Message.EscapeMarkup());
		}
	}
}