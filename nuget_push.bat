start /B /WAIT "" "src\.nuget\NuGet.exe" install src\.nuget\packages.config -o src\packages

powershell.exe -noexit -command "%~dp0build.ps1 $task NuGet-Push"