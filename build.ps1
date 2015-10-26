param([Parameter(Position = 0,Mandatory = 0)][string]$buildFile = 'default.ps1',
	  [Parameter(Position = 1,Mandatory = 0)][string]$task = "SmokeTest",
      [Parameter(Position = 2, Mandatory = 0)][System.Collections.Hashtable]$properties = @{})

$directory = Get-ChildItem -recurse | Where-Object {$_.PSIsContainer -eq $true -and $_.Name.StartsWith("psake")}
$path = "$($directory.FullName)\tools\psake.psm1"
Remove-Module [p]sake
Import-Module ($path)
Invoke-psake $buildFile @($task) $properties
