$base_directory = Resolve-Path .
$sourcePackage = "$base_directory\DatabaseTemplate.zip"
$unzipUtil = "$base_directory\Unzip.exe"
$targetfolder = "$env:AD?_RavenDrive"
if ($targetfolder -eq "") {
	$targetfolder = "$env:LR?_RavenDrive"
}
if ($targetfolder -eq "") {
	$targetfolder = "$base_directory\DatabaseTemplate"
}
Write-Host "Target Folder for DB is $targetfolder and source archive is at $sourcePackage"

# Unzip to target location
if ($targetfolder -and (Test-Path $targetfolder) -and !(Test-Path "$targetfolder\Database")) {
	Write-Host "Unpacking..."
	$allArgs = @("$sourcePackage", "-d", "$targetfolder")
	& $unzipUtil $allArgs
}
else
{
	Write-Host "Database found or destination is invalid. Skipping..."
}