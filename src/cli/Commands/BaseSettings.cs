﻿using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Data.SqlClient;

namespace Sql.Migrate.Cli.Commands;

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

	[CommandOption("--verbose")]
	[Description("Verbose output.")]
	public bool Verbose { get; set; }

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

	public virtual BaseSettings ForMaster()
	{
		return new BaseSettings
		{
			Server = Server,
			Database = "master",
			Schema = Schema,
			UserId = UserId,
			Password = Password,
			Verbose = true
		};
	}

	private static void IsSet(BaseSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.Server);
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.Database);
	}
}
