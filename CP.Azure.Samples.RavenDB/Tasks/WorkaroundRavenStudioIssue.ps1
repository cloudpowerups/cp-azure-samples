$targetFolder = "D:\Packages"

$acl = Get-Acl $targetFolder
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Network Service","ReadAndExecute, Synchronize", "ContainerInherit, ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule)
Set-Acl $targetFolder $acl