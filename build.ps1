[CmdletBinding()]
Param([string]$script = "build.cake",
      [string]$target = "Default",
      [ValidateSet("Release", "Debug")][string]$configuration = "Release",
      [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")][string]$verbosity = "Verbose",
      [Parameter(Position=0, Mandatory=$false, ValueFromRemainingArguments=$true)][string[]]$arguments)

function Get-BuildNumber {
    $revision = git rev-list HEAD --count
    $revision = $revision.trimEnd('\n')

    return $revision
}

Write-Host "preparing to run build script..."

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$CAKE_VERSION = "0.22.2"
$CAKE_FEED = "https://www.myget.org/F/cake/api/v3/index.json"
$CAKE_DLL = "$TOOLS_DIR/cake.coreclr/$CAKE_VERSION/Cake.dll"

if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    Write-Verbose -Message "creating tools directory..."
    New-Item -Path $TOOLS_DIR -Type directory | out-null
}

if (!(Test-Path $CAKE_DLL)) {
    if (!(Test-Path "$TOOLS_DIR\project.csproj")) {
        Write-Verbose -Message "creating project.csproj..."
        echo '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>netstandard1.5</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include="Cake.CoreCLR" Version="'$CAKE_VERSION'" /></ItemGroup></Project>' > "$TOOLS_DIR\project.csproj"
    }

    $Exp = "dotnet restore `"$TOOLS_DIR\project.csproj`" --packages `"$TOOLS_DIR`" --source `"$CAKE_FEED`""
    $NuGetOutput = Invoke-Expression "& $Exp" | Out-String
    Write-Verbose -Message $NuGetOutput

    if (!(Test-Path $CAKE_DLL)) {
        Throw "could not find cake.dll at $CAKE_DLL."
        exit 1
    }
}

$buildnumber = Get-BuildNumber

Write-Host "running build script..."
Invoke-Expression "& dotnet `"$CAKE_DLL`" `"$script`" -target=`"$target`" -configuration=`"$configuration`" -buildnumber=`"$buildnumber`" -verbosity=`"$verbosity`" $arguments"
exit $LASTEXITCODE