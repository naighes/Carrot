function Get-SolutionInfo {
    param([Parameter(Position = 0, Mandatory = $true)][String]$commonAssemblyInfoPath)

    $commonAssemblyInfoContent = Get-Content $commonAssemblyInfoPath
	
    return @{
        Company = Get-AssemblyInfoAttributeStringValue $commonAssemblyInfoContent "AssemblyCompany";
        Copyright = Get-AssemblyInfoAttributeStringValue $commonAssemblyInfoContent "AssemblyCopyright";
        ComVisible = Get-AssemblyInfoAttributeNonStringValue $commonAssemblyInfoContent "ComVisible";
        Product = Get-AssemblyInfoAttributeStringValue $commonAssemblyInfoContent "AssemblyProduct";
        CLSCompliant = Get-AssemblyInfoAttributeNonStringValue $commonAssemblyInfoContent "CLSCompliant";
        DelaySign = Get-AssemblyInfoAttributeNonStringValue $commonAssemblyInfoContent "AssemblyDelaySign";
        NeutralResourcesLanguage = Get-AssemblyInfoAttributeStringValue $commonAssemblyInfoContent "NeutralResourcesLanguage";
    }
}

function Get-ProjectsInfo {
    [CmdletBinding()]
    param([Parameter(Position = 0, Mandatory = $true)][String]$directory,
          [Parameter(Position = 1, Mandatory = $false)][String]$commonAssemblyInfoPath,
          [Parameter(Position = 2, Mandatory = $false)][String]$projectExt = "csproj")

    $paths = New-Object System.Collections.Generic.List[PSObject]
    $files = @(Get-ChildItem $directory -Recurse -Filter "*.$projectExt")
    $solutionInfo = Get-SolutionInfo $commonAssemblyInfoPath
	
    foreach ($file in $files) {
        $document = New-Object XML
        $document.Load($file.FullName)
        $projectName = $file.Name.Replace(".$projectExt", "")
        $assemblyInfoPath = "$($file.Directory)\Properties\AssemblyInfo.cs";
        $content = Get-Content $assemblyInfoPath
        $paths.Add(@{ 
            ProjectFile = $file.Name;
            ProjectName = $projectName;
            ProjectFilePath = $file.FullName;
            ProjectDirectory = $file.Directory.FullName;
            BinDirectory = "$($file.Directory)\bin\";
            ObjDirectory = "$($file.Directory)\obj\";
            PackagesConfigPath = "$($file.Directory.FullName)\packages.config";
            NuspecFile = "$($file.Directory.FullName)\$($projectName).nuspec";
            AssemblyName = ($document.Project.PropertyGroup | where { $_.AssemblyName -ne $null }).AssemblyName;
            IsTestProject = ($document.Project.ItemGroup.Reference.Include | where { $_.StartsWith("xunit") }) -ne $null;
            AssemblyInfoPath = $assemblyInfoPath;
            Company = $("$solutionInfo.Company");
            Copyright = $("$solutionInfo.Copyright");
            ComVisible = $("$solutionInfo.ComVisible");
            Product = $("$solutionInfo.Product");
            CLSCompliant = $("$solutionInfo.CLSCompliant");
            DelaySign = $("$solutionInfo.DelaySign");
            NeutralResourcesLanguage = $("$solutionInfo.NeutralResourcesLanguage");
            Title = Get-AssemblyInfoAttributeStringValue $content "AssemblyTitle";
            Guid = Get-AssemblyInfoAttributeStringValue $content "Guid";
            Description = Get-AssemblyInfoAttributeStringValue $content "AssemblyDescription";
            InternalsVisibleTo = Get-AssemblyInfoAttributeStringValue $content "InternalsVisibleTo";
        })
    }

    return $paths;
}

function Get-BuildNumber {
    $revision = git rev-list HEAD --count
    $revision = $revision.trimEnd('\n')
    return $revision
}

function Exit-Build {
    [CmdletBinding()]
    param([Parameter(Position = 0, Mandatory = $true)][String]$message)

    Write-Host $("`nExiting build because task [{0}] failed.`n->`t$message.`n" -f $psake.context.Peek().currentTaskName) -ForegroundColor Red

    Exit
}

function Get-SolutionFile {
    param([Parameter(Position = 0, Mandatory = $true)][String]$root)
    return (@(Get-ChildItem "$root" -Filter "*.sln") | Select-Object -First 1)
}

function Get-FileVersion {
    param([Parameter(Position = 0, Mandatory = $true)][String]$productVersion,
          [Parameter(Position = 1, Mandatory = $true)][String]$patchVersion,
          [Parameter(Position = 2, Mandatory = $true)][String]$buildNumber)
    return $assemblyFileVersion = $productVersion + "." + $patchVersion + "." + $buildNumber
}

function Get-InformationalVersion {
    param([Parameter(Position = 0, Mandatory = $true)][String]$productVersion,
          [Parameter(Position = 1, Mandatory = $true)][String]$patchVersion,
          [Parameter(Position = 2, Mandatory = $true)][String]$buildNumber,
          [Parameter(Position = 3, Mandatory = $false)][String]$preRelease)
    $assemblyInformationalVersion = $productVersion + "." + $patchVersion + "." + $buildNumber

    if ($preRelease -ne "") {
        $assemblyInformationalVersion = $assemblyInformationalVersion + "-" + $preRelease
    }

    return $assemblyInformationalVersion
}

function Get-AssemblyInfoAttributeNonStringValue([String]$Content, [String]$Key) {
    $pattern = "(?:\s*)\[(?:\s*)assembly(?:\s*):(?:\s*)$Key(?:\s*)\((?:\s*)([^)]*)(?:\s*)\)(?:\s*)\](?:\s)*"
    $Content -match $pattern > $null
	
    if ($null -eq $matches) {
        return "false"
    }

    return $matches[1]
}

function Get-AssemblyInfoAttributeStringValue([String]$Content, [String]$Key) {
    $pattern = "(?:\s*)\[(?:\s*)assembly(?:\s*):(?:\s*)$Key(?:\s*)\((?:\s*)""(?:\s*)([^""]*)(?:\s*)""(?:\s*)\)(?:\s*)\](?:\s)*"
    $matchesLines = [regex]::matches($Content, $pattern, "Multiline")

    if ($null -eq $matchesLines) {
        return ""
    }

    $result = @()

    foreach ($line in $matchesLines) {
        $line -match $pattern > $null
        $result += $matches[1]
    }

    return $result
}

function Add-AssemblyInfoStringItem([string]$Key, [string]$Value) {
    if ($Value -ne "") {
        return "[assembly: $Key(""$Value"")]
"
    }

    return ""
}

function Add-AssemblyInfoNonStringItem([string]$Key, [string]$Value) {
    if ($Value -ne "") {
        return "[assembly: $Key($Value)]
"
    }
	
    return ""
}

function Generate-AssemblyInfo {
    param ([string]$assemblyTitle,
           [string]$guid,
           [string]$description,
           [string[]]$internalsVisibleTo,
           [string]$path = $(throw "'path' is a required parameter."))

    $content = "using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

"

    $content += Add-AssemblyInfoStringItem "AssemblyTitle" $assemblyTitle
    $content += Add-AssemblyInfoStringItem "Guid" $guid
    $content += Add-AssemblyInfoStringItem "AssemblyDescription" $description
	
    foreach ($ivt in $internalsVisibleTo) {
        $content += Add-AssemblyInfoStringItem "InternalsVisibleTo" $ivt
    }
	
    Write-Host "Generating AssemblyInfo.cs in '$path'."
    $content | Out-File -Encoding UTF8 $path
}

function Generate-CommonAssemblyInfo {
    param ([string]$clsCompliant,
           [string]$configuration,
           [string]$assemblyCompany,
           [string]$assemblyProduct,
           [string]$assemblyCopyright,
           [string]$assemblyVersion,
           [string]$assemblyFileVersion,
           [string]$assemblyInformationalVersion,
           [string]$comVisible,
           [string]$neutralResourcesLanguage,
           [string]$assemblyDelaySign,
           [string]$path = $(throw "'path' is a required parameter."))

    if ($assemblyInformationalVersion -eq "") {
        $assemblyInformationalVersion = $assemblyFileVersion
    }
	
    $content = "using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

"

    $content += Add-AssemblyInfoStringItem "AssemblyCompany" $assemblyCompany
    $content += Add-AssemblyInfoStringItem "AssemblyCopyright" $assemblyCopyright
    $content += Add-AssemblyInfoNonStringItem "ComVisible" $comVisible
    $content += Add-AssemblyInfoStringItem "AssemblyVersion" $assemblyVersion
    $content += Add-AssemblyInfoStringItem "AssemblyFileVersion" $assemblyFileVersion
    $content += Add-AssemblyInfoStringItem "AssemblyInformationalVersion" $assemblyInformationalVersion
    $content += Add-AssemblyInfoStringItem "AssemblyProduct" $assemblyProduct
    $content += Add-AssemblyInfoNonStringItem "CLSCompliant" $clsCompliant
    $content += Add-AssemblyInfoStringItem "AssemblyConfiguration" $configuration
    $content += Add-AssemblyInfoNonStringItem "AssemblyDelaySign" $assemblyDelaySign
    $content += Add-AssemblyInfoStringItem "NeutralResourcesLanguage" $neutralResourcesLanguage	
	
    Write-Host "Generating SolutionInfo.cs in '$path'."
    $content | Out-File -Encoding UTF8 $path
}
