SET CLI=..\src\cli\bin\Debug\net6.0\sql-migrate-cli.exe
SET SERVER=.
SET DB_NAME=AdventureWorks

:: Drop a database if it exists
%CLI% drop -s %SERVER% -d %DB_NAME%

:: Create a database if it does not already exist
%CLI% create -s %SERVER% -d %DB_NAME%

:: Backup database
:: sqlcmd -S %SERVER% -Q "BACKUP DATABASE %DB_NAME% TO DISK = '%~dp0AdventureWorks\backup\AdventureWorks.bak' WITH NOFORMAT, NOINIT, NAME = 'Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

:: Restore database
:: %CLI% restore -s %SERVER% -d %DB_NAME% -f ./AdventureWorks/backup/AdventureWorks.bak --verbose
sqlcmd -S %SERVER% -Q "RESTORE DATABASE %DB_NAME% FROM DISK = '%~dp0AdventureWorks\backup\AdventureWorks.bak' WITH FILE = 1, NOUNLOAD, REPLACE, STATS = 5"

::all stored procedures, functions, views and triggers from database and save to individual files in `folder`
::%CLI% export -s %SERVER% -d %DB_NAME% -o ./AdventureWorks -f

::migrate database schema - apply one-time scripts
%CLI% migrate -s %SERVER% -d %DB_NAME% -f ./AdventureWorks/migrations

::deploy procedures, views, functions and triggers
%CLI% import -s %SERVER% -d %DB_NAME% -f ./AdventureWorks/Functions --verbose
%CLI% import -s %SERVER% -d %DB_NAME% -f ./AdventureWorks/Procedures --verbose
%CLI% import -s %SERVER% -d %DB_NAME% -f ./AdventureWorks/Views --verbose
%CLI% import -s %SERVER% -d %DB_NAME% -f ./AdventureWorks/Triggers --verbose



