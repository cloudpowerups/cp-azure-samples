REM Extracting database
powershell -NoProfile -ExecutionPolicy Unrestricted -Command "& '%~dp0\ExtractDatabase.ps1' %*"
REM Granting permissions to workaround RavenStudio bug
powershell -NoProfile -ExecutionPolicy Unrestricted -Command "& '%~dp0\WorkaroundRavenStudioIssue.ps1' %*"
REM Restarting W3SVC to apply new environment variables
call RestartW3SVC.cmd
REM Setting up replication with master
if exist CP.RavenDB.ReplicationSetup.exe start CP.RavenDB.ReplicationSetup.exe