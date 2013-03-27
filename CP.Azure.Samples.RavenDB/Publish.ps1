$msbuild = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe"
$base_directory = Resolve-Path .
$configuration = "Release"
$package = "$base_directory\RavenMasterSlaveReads.factory.exe"
$replicationSetupProjectOutput = "$base_directory\CP.RavenDB.ReplicationSetup\bin\$configuration"

$latest_build = "$base_directory\output"
$replicationSetupBinaries = "$latest_build\replication"
$ravenDBWebBinaries = "$base_directory\RavenWeb"
$tasksBinaries = "$base_directory\Tasks"
$dbTemplates = "$base_directory\DBTemplates"

$deploy = "$base_directory\deploy"

#############
#### Step 1: Clean up
foreach ($dirToClean in ($latest_build, $deploy)) {
	if (Test-Path $dirToClean) {
		rmdir $dirToClean -recurse -force
	}
}

#############
#### Step 2: Publish project into "output" folder

# Build solution
$allArgs = @("/m", "CP.Azure.Samples.RavenDB.sln", "/p:Configuration=$configuration", "/v:Minimal")
& $msbuild $allArgs

# Publish Engine
md -Path "$replicationSetupBinaries"
copy -Path "$replicationSetupProjectOutput\*" -recurse -destination "$replicationSetupBinaries\" -Force

##############
#### Step 3: Create a package ready for Azure deployment

# Open package
if (Test-Path $package) {
	$allArgs = @("-unpack", "-target=`"$deploy`"")
	& $package $allArgs
}

# Locate paths
$masterWebTarget = (Get-ChildItem -Path "$deploy\RavenDB-Master" -Filter RavenWeb.txt -Recurse).DirectoryName
$slaveWebTarget = (Get-ChildItem -Path "$deploy\RavenDB-Slave" -Filter RavenWeb.txt -Recurse).DirectoryName
$masterTasksTarget = (Get-ChildItem -Path "$deploy\RavenDB-Master" -Filter CP.Azure.DeliveryPackage.Puppeteer.dll -Recurse).DirectoryName
$slaveTasksTarget = (Get-ChildItem -Path "$deploy\RavenDB-Slave" -Filter CP.Azure.DeliveryPackage.Puppeteer.dll -Recurse).DirectoryName

# Inject RavenWeb for Master
if ($masterWebTarget -and (Test-Path $masterWebTarget)) {
	Remove-Item "$masterWebTarget\*" -Recurse
	Copy-Item "$ravenDBWebBinaries\*" "$masterWebTarget\" -recurse
	Copy-Item "$masterWebTarget\Master.web.config" "$masterWebTarget\web.config" -force
}

# Inject RavenWeb for Slave
if ($slaveWebTarget -and (Test-Path $slaveWebTarget)) {
	Remove-Item "$slaveWebTarget\*" -Recurse
	Copy-Item "$ravenDBWebBinaries\*" "$slaveWebTarget\" -recurse
	Copy-Item "$slaveWebTarget\Slave.web.config" "$slaveWebTarget\web.config" -force
}

# Inject Tasks for Master
if ($masterTasksTarget -and (Test-Path $masterTasksTarget)) {
	$masterTasksTarget = "$masterTasksTarget\Tasks"
	md -Path $masterTasksTarget
	Copy-Item "$tasksBinaries\*" "$masterTasksTarget\" -recurse
	Copy-Item "$dbTemplates\MasterDatabase.zip" "$masterTasksTarget\DatabaseTemplate.zip"
}

# Inject Tasks for Slave
if ($slaveTasksTarget -and (Test-Path $slaveTasksTarget)) {
	$slaveTasksTarget = "$slaveTasksTarget\Tasks"
	md -Path $slaveTasksTarget
	Copy-Item "$tasksBinaries\*" "$slaveTasksTarget\" -recurse
	Copy-Item "$replicationSetupBinaries\*" "$slaveTasksTarget\" -recurse
	Copy-Item "$dbTemplates\SlaveDatabase.zip" "$slaveTasksTarget\DatabaseTemplate.zip"
}

# Produce Azure package
if (Test-Path $package) {
	$allArgs = @("-repack", "-target=`"$deploy`"")
	& $package $allArgs
}
