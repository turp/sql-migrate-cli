![Dejavu](dejavu-logo-wide.png)

# Deploy

An API for CI/CD tasks.

## [Cake](https://cakebuild.net) Integration

Add the following lines to the top of your cake script. You can get the latest package version numbers by browsing the available dejavu.Deploy nuget packages on [Artifactory](https://ubit-artifactory-or.intel.com/artifactory/webapp/#/search/package/eyJwYWNrYWdlUGF5bG9hZCI6W3siaWQiOiJudWdldFBhY2thZ2VJZCIsInZhbHVlcyI6WyJkZWphdnUuRGVwbG95Il19XSwic2VsZWN0ZWRQYWNrYWdlVHlwZSI6eyJpZCI6Im51Z2V0IiwiZGlzcGxheU5hbWUiOiJOdUdldCIsImljb24iOiJudWdldCJ9fQ==). You can sort descending on the Modified column.

```csharp
#addin "nuget:?package=dejavu.Deploy&version=2020.5.13.1656"
using dejavu.Deploy;  // So you don't have to fully qualify the types.
```

Add a cake.config file next to your build.cake file that has the following settings:

```bash
    [Nuget]
    Source=https://api.nuget.org/v3/index.json;https://ubit-artifactory-or.intel.com/artifactory/api/nuget/dejavu-nuget-local
    LoadDependencies=true
```

## API Documentation

### Build Version

**Get the current date and time as a version number.** The format will be `yyyy.M.d.Hmm`

```csharp
string BuildVersion.CurrentDateTime { get; }
```

**Convert a version number dots to underscore.**

```csharp
string BuildVersion.ConvertToUnderscore(string version);
```

### Database Settings

Use an instance of `DatabaseSettings` to pass the required parameters to the `Database` and `DacServices` methods.

```csharp
public class DatabaseSettings
{
    public string Server { get; set; }
    public string Database { get; set; }
    // optional: defaults to dbo
    public string Schema { get; set; }
    // optional: if not provided, defaults to integrated security
    public string UserId { get; set; }
    public string Password { get; set; }
}
```

### Database

**Create a database if it does not already exist.**

```csharp
void Database.Create(DatabaseSettings settings, bool verbose = false);

// Example
Database.Create(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    });
```

**Drop a database if it exists.**

```csharp
void Database.Drop(DatabaseSettings settings, bool verbose = false);

// Example
Database.Drop(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    });
```

**Perform a database backup to file `backupFilePath`**

```csharp
void Database.Backup(DatabaseSettings settings, string backupFilePath, bool verbose = false);

// Example
Database.Backup(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    // remember that this file is relative to the server
    // that the backup command is being executed on
    backupFilePath: @"D:\backups\my_database.bak"
);
```

**Perform a database restore from file `backupFilePath`.**

```csharp
void Database.Restore(DatabaseSettings settings, string backupFilePath, bool verbose = false);

// Example
Database.Restore(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    // file must be accessible from the server where
    // the restore is being performed
    backupFilePath: @"\\server\path\backup_file.bak"
);
```

**Extract all stored procedures, functions, views and triggers from database and save to individual files in `folder`.**

```csharp
void Database.Extract(DatabaseSettings settings, string folder, bool overwriteExisting = false);

// Example
Database.Extract(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    folder: @"./database",
    overwriteExisting: false
);
```

**Create a linked server. This will drop the linked if it already exists and recreate it. Requires server admin privileges. From a security standpoint, you should not store login and passwords inside the script. Pass this information into the script via environment variables in CI/CD**

```csharp
void Database.LinkedServer(DatabaseSettings settings, LinkedServerSettings linkedServer, bool verbose = false);

// Example
Database.LinkedServer(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    linkedServer: new LinkedServer {
        Name = "MyLinkedServer",
        Server = "remote_server.intel.com,3181",
        Database = "remote_db_name",
        Login = "remote_db_login",
        Password = "remote_db_password"
    }
);
```

**Drop any view, stored procedure, or function** that has a create date less than `minDate`.

```csharp
void Database.DropOldObjects(DatabaseSettings settings, DateTime olderThan, bool verbose = false);

// Example
Database.DropOldObjects(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    olderThan: DateTime.Now.AddMinutes(-30)
);
```

**Execute all SQL scripts found in the collection of `filePaths`. Each folder will be executed in the order specified. The scripts in each folder be executed in the alphanumeric order**

```csharp
void Database.ExecuteFiles(DatabaseSettings settings, IEnumerable<string> filePaths, bool verbose = false);

// Example
Database.ExecuteFiles(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    filePaths: new string[]
    {
        // include specific files that need to run first
        @"database\views\vw_workers.SQL"
        @"database\procedures",
        @"database\views",
        @"database\functions",
        // or last
        @"database\security\add_permissions.SQL"
    }
);
```

**Execute a SQL script file. File may contain `GO` statements.**

```csharp
void Database.ExecuteFile(DatabaseSettings settings, string filePath, bool verbose = false);

// Example
Database.ExecuteFile(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    filePath: @".\database\myscript.sql"
);
```

**Execute a SQL command, or a list of SQL commands, that returns no value.**

```csharp
void Database.ExecuteNonQuery(DatabaseSettings settings, string sql, bool verbose = false);
void Database.ExecuteNonQuery(DatabaseSettings settings, IEnumerable<string> batches, bool verbose = false);

// Example
Database.ExecuteNonQuery(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    sql: "UPDATE MyTable SET Col1 = 2112;"
);
```

**Execute a SQL command to get back a single value.**

```csharp
T Database.ExecuteScalar<T>(DatabaseSettings settings, string sql, bool verbose = false);

// Example
var n = Database.ExecuteScalar<int>(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    sql: "SELECT 2112;"
);

// Example
Database.ExecuteScalar<DateTime>(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    sql: "SELECT GETDATE();"
);
```

**Retrieve data from database**

```csharp
IEnumerable<IDataRecord> Database.ExecuteReader(DatabaseSettings settings, string sql, bool verbose = false);

// Example
var records = Database.ExecuteReader(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    sql: "SELECT name FROM sys.databases"
);

foreach (IDataRecord record in records)
{
    var name = record.GetString(0);
}
```

### Migrate Database

**Execute SQL migration scripts.** The file name should be formatted as `##_Description.sql`, where `##` is the schema version and `Description` is any descriptive text. The files in the `folder` will be executed in order by version number and then file name starting from the latest version number stored in the `dbo.SchemaVersion` table. After each script is executed, the `dbo.SchemaVersion` table is updated. The final version number is returned.

```csharp
int SqlServer.Migrate.Execute(DatabaseSettings settings, string folder, bool verbose = false);

// Example
SqlServer.Migrate.Execute(
    settings: new DatabaseSettings
    {
        Server = "MyServer",
        Database = "MyDatabase"
    },
    folder: @".\database\migrations"
);
```

**Get the current version number stored in the `dbo.SchemaVersion` table.**

```csharp
int SqlServer.Migrate.GetCurrentVersion(DatabaseSettings settings);
```

### Combine Files

All files in the `IncludeFolders` list, including subdirectories, matching the `FilePattern` will be concatenated together into a single file at `ToFilePath`.

```csharp
void Files.Combine(CombineFilesSettings settings, bool verbose = false);

// Example
Files.Combine(new CombineFilesSettings
{
    ToFilePath = "CombinedFile.SQL",
    IncludeFolders = new []
    {
        $"{dbScriptPath}/Functions",
        $"{dbScriptPath}/Views",
        $"{dbScriptPath}/Procedures"
    },
    FilePatterns = new [] { "*.sql" }
});
```

### Secrets

**Ask for an environment variable if it does not exist.** If the environment variable does not exist, a prompt will ask the user to input the value. If the environment variable was set, this return true. If the environment variable already exists, this returns false.

```csharp
bool Secrets.AskForEnvironmentVariableIfNotExists(string name, bool allowBlank = false);
```

**Build a secrets config file.** For each name in `environmentVariableNames` that has a value set in the current environment, it will be added to a config file located at `filePath`. The `key` will be the environment variable name minus the `namePrefix`. `namePrevix` can be blank or null.

```csharp
void Secrets.BuildSecretsConfig(string filePath, string namePrefix, IEnumerable<string> environmentVariableNames);
```

### Secrets

**Encrypt a secrets file.** Creates the destination file and does not remove the source file.

```csharp
void Secrets.EncryptFile(string srcFilePath, string destFilePath, string key);
```

**Decrypt a secrets file.** Creates the destination file and does not remove the source file.

```csharp
void Secrets.DecryptFile(string srcFilePath, string destFilePath, string key);
```

**Encrypt a string.** Returns a Base64 encoded value.

```csharp
string Secrets.EncryptString(string value, string key);
```

**Decrypt a string.** Takes a Base64 encoded value.

```csharp
string Secrets.DecryptString(string value, string key);
```