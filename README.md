# SQL Server Migration CLI

An API for common database CI/CD tasks.

## Deploy

```
docker build --tag amr-registry.caas.intel.com/ducttape/sql-migrate-cli "C:\_dots\sql-migrate\src" -f "C:\_dots\sql-migrate\src\cli\Dockerfile"

docker push amr-registry.caas.intel.com/ducttape/sql-migrate-cli
```

## Database

### Create a database if it does not already exist

```
.\sql-migrate-cli.exe create -s SERVER_NAME -d DB_NAME --verbose
```

### Drop a database if it exists

```
.\sql-migrate-cli.exe drop -s SERVER_NAME -d DB_NAME
```

### Backup database backup to file `backupFilePath`

```
.\sql-migrate-cli.exe backup -s SERVER_NAME -d DB_NAME -b ./folder_name --force
```

### Restore database restore from file `backupFilePath`

Note: File must be accessible from the server where the restore is being performed.

```
.\sql-migrate-cli.exe restore -s SERVER_NAME -d DB_NAME -b ./folder_name --force
```

## Extract 

all stored procedures, functions, views and triggers from database and save to individual files in `folder`

```
.\sql-migrate-cli.exe export -s SERVER_NAME -d DB_NAME -o ./folder_name --force
```

### Execute a SQL script file. File may contain `GO` statements.**

```
.\sql-migrate-cli.exe execute -s SERVER_NAME -d DB_NAME -o ./database/myscript.sql --force
```

### Migrate Database

**Execute SQL migration scripts.** The file name should be formatted as `##_Description.sql`, where `##` is the schema version and `Description` is any descriptive text. The files in the `folder` will be executed in order by version number and then file name starting from the latest version number stored in the `dbo.SchemaVersion` table. After each script is executed, the `dbo.SchemaVersion` table is updated. The final version number is returned.

```
.\sql-migrate-cli.exe migrate -s SERVER_NAME -d DB_NAME -o ./database/migrations
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


**Create a linked server. This will drop the linked server if it already exists and recreate it. Requires server admin privileges. From a security standpoint, you should not store login and passwords inside the script. Pass this information into the script via environment variables in CI/CD**

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

### Build Version

**Get the current date and time as a version number.** The format will be `yyyy.M.d.Hmm`

```csharp
string BuildVersion.CurrentDateTime { get; }
```

**Convert a version number dots to underscore.**

```csharp
string BuildVersion.ConvertToUnderscore(string version);
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

**Get the current version number stored in the `dbo.SchemaVersion` table.**

```csharp
int SqlServer.Migrate.GetCurrentVersion(DatabaseSettings settings);
```
