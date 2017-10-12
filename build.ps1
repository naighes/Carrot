[CmdletBinding()]
Param([string]$Script = "build.cake",
      [string]$Target = "Default",
      [ValidateSet("Release", "Debug")]
      [string]$Configuration = "Release",
      [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
      [string]$Verbosity = "Verbose",
      [switch]$Experimental,
      [Alias("DryRun","Noop")]
      [switch]$WhatIf,
      [switch]$Mono,
      [switch]$SkipToolPackageRestore,
      [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
      [string[]]$ScriptArgs)

[Reflection.Assembly]::LoadWithPartialName("System.Security") | Out-Null
function MD5HashFile([string] $filePath)
{
    if ([string]::IsNullOrEmpty($filePath) -or !(Test-Path $filePath -PathType Leaf))
    {
        return $null
    }

    [System.IO.Stream] $file = $null;
    [System.Security.Cryptography.MD5] $md5 = $null;
    try
    {
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $file = [System.IO.File]::OpenRead($filePath)
        return [System.BitConverter]::ToString($md5.ComputeHash($file))
    }
    finally
    {
        if ($file -ne $null)
        {
            $file.Dispose()
        }
    }
}

Write-Host "preparing to run build script..."

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
# $CAKE_EXE = Join-Path $TOOLS_DIR "Cake/Cake.exe"
$NUGET_URL = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"
$PACKAGES_CONFIG_MD5 = Join-Path $TOOLS_DIR "packages.config.md5sum"
$CAKE_VERSION = "0.22.2"
$CAKE_FEED = "https://www.myget.org/F/cake/api/v3/index.json"
$CAKE_DLL = "$TOOLS_DIR/cake.coreclr/$CAKE_VERSION/Cake.dll"

# Should we use mono?
$UseMono = "";

if ($Mono.IsPresent) {
    Write-Verbose -Message "using the Mono based scripting engine."
    $UseMono = "-mono"
}

# Should we use the new Roslyn?
$UseExperimental = "";

if ($Experimental.IsPresent -and !($Mono.IsPresent)) {
    Write-Verbose -Message "using experimental version of Roslyn."
    $UseExperimental = "-experimental"
}

# Is this a dry run?
$UseDryRun = "";

if ($WhatIf.IsPresent) {
    $UseDryRun = "-dryrun"
}

# Make sure tools folder exists
if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    Write-Verbose -Message "creating tools directory..."
    New-Item -Path $TOOLS_DIR -Type directory | out-null
}

# Make sure that packages.config exist.
# if (!(Test-Path $PACKAGES_CONFIG)) {
    # Write-Verbose -Message "downloading packages.config..."

    # try { (New-Object System.Net.WebClient).DownloadFile("https://cakebuild.net/download/bootstrapper/packages", $PACKAGES_CONFIG) } catch {
        # Throw "could not download packages.config."
    # }
# }

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

# Try find NuGet.exe in path if not exists
# if (!(Test-Path $NUGET_EXE)) {
    # Write-Verbose -Message "trying to find nuget.exe in PATH..."
    # $existingPaths = $Env:Path -Split ';' | Where-Object { (![string]::IsNullOrEmpty($_)) -and (Test-Path $_ -PathType Container) }
    # $NUGET_EXE_IN_PATH = Get-ChildItem -Path $existingPaths -Filter "nuget.exe" | Select -First 1

    # if ($NUGET_EXE_IN_PATH -ne $null -and (Test-Path $NUGET_EXE_IN_PATH.FullName)) {
        # Write-Verbose -Message "Found in PATH at $($NUGET_EXE_IN_PATH.FullName)."
        # $NUGET_EXE = $NUGET_EXE_IN_PATH.FullName
    # }
# }

# # Try download NuGet.exe if not exists
# if (!(Test-Path $NUGET_EXE)) {
    # Write-Verbose -Message "downloading NuGet.exe..."

    # try {
        # (New-Object System.Net.WebClient).DownloadFile($NUGET_URL, $NUGET_EXE)
    # } catch {
        # Throw "Could not download NuGet.exe."
    # }
# }

# Save nuget.exe path to environment to be available to child processed
# $ENV:NUGET_EXE = $NUGET_EXE

# Restore tools from NuGet?
# if(-Not $SkipToolPackageRestore.IsPresent) {
    # Push-Location
    # Set-Location $TOOLS_DIR

    # # Check for changes in packages.config and remove installed tools if true.
    # [string] $md5Hash = MD5HashFile($PACKAGES_CONFIG)
    # if((!(Test-Path $PACKAGES_CONFIG_MD5)) -Or
      # ($md5Hash -ne (Get-Content $PACKAGES_CONFIG_MD5 ))) {
        # Write-Verbose -Message "Missing or changed package.config hash..."
        # Remove-Item * -Recurse -Exclude packages.config,nuget.exe
    # }

    # Write-Verbose -Message "Restoring tools from NuGet..."
    # $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$TOOLS_DIR`""

    # if ($LASTEXITCODE -ne 0) {
        # Throw "An error occured while restoring NuGet tools."
    # }
    # else
    # {
        # $md5Hash | Out-File $PACKAGES_CONFIG_MD5 -Encoding "ASCII"
    # }
    # Write-Verbose -Message ($NuGetOutput | out-string)
    # Pop-Location
# }

# Make sure that Cake has been installed.
# if (!(Test-Path $CAKE_EXE)) {
    # Throw "could not find Cake.exe at $CAKE_EXE"
# }

# Start Cake
Write-Host "running build script..."
Invoke-Expression "& dotnet `"$CAKE_DLL`" `"$Script`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" $UseMono $UseDryRun $UseExperimental $ScriptArgs"
exit $LASTEXITCODE