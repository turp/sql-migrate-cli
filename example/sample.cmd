SET CLI=..\src\cli\bin\Debug\net6.0\sql-migrate-cli.exe
SET SERVER=.
SET DB_NAME=AdventureWorks

:: Drop a database if it exists
%CLI% drop -s %SERVER% -d %DB_NAME%

:: Create a database if it does not already exist
%CLI% create -s %SERVER% -d %DB_NAME%

:: Restore database
%CLI% restore -s %SERVER% -d %DB_NAME% -f ./AdventureWorks/backup/AdventureWorks.bak --verbose

::all stored procedures, functions, views and triggers from database and save to individual files in `folder`
%CLI% export -s %SERVER% -d %DB_NAME% -o ./AdventureWorks -f

::```
::