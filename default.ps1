import-module ".\extras.psm1"
$framework = "4.0"

properties {
	$baseDir  = resolve-path .  	
	$srcDir = "$baseDir\src"
	$nugetPackagesDirectoryName = "packages"
	$nugetPackagesDirectoryPath = "$srcDir\$nugetPackagesDirectoryName"
	$config = "Release"
	$commonAssemblyInfoPath = "$srcDir\CommonAssemblyInfo.cs"
	
	$msbuildVerbosity = "normal"
    $msbuildCpuCount = [System.Environment]::ProcessorCount / 2
    $msbuildParralel = $true
	$platform = "AnyCPU"
	
    $productVersion = "0"
	$patchVersion = "0"
		
	$nugetOutputPath = "$baseDir\nuget-out"
	
	$preRelease = "alpha"
	$buildNumber = Get-BuildNumber
}

Task SmokeTest {
	Write-Host "Up & running on '$baseDir'!"
}

Task PrepareBinaries -Depends Clean, NuGet-Restore, Compile, Run-UnitTests

Task CreateRelease -Depends AssemblyInfo-Generate, PrepareBinaries

Task NuGet-Push -Depends CreateRelease, NuGet-CreatePackages #, NuGet-Publish

Task Clean {
	Write-Host $bn
	
	if (Test-Path $nugetPackagesDirectoryPath) {
		try {
			Write-Host "Cleaning up NuGet packages directory at '$nugetPackagesDirectoryPath'."
			
			Get-ChildItem -Path $nugetPackagesDirectoryPath | ?{ $_.PSIsContainer } | ForEach-Object {
				if (-not($_.Name.StartsWith("psake", "CurrentCultureIgnoreCase"))) {
					Remove-Item $_.FullName -Force -Recurse -ErrorAction SilentlyContinue
				}
			}
		}
		catch {
			Exit-Build "Failed to deleting '$nugetPackagesDirectoryPath' folder's content."
		}
	}
	
	if (Test-Path $nugetOutputPath) {
		try {
			Write-Host "Cleaning up NuGet packages output directory at '$nugetOutputPath'."
			
			if (Test-Path $nugetOutputPath) {
				Remove-Item $nugetOutputPath -Force -Recurse -ErrorAction SilentlyContinue
			}
		}
		catch {
			Exit-Build "Failed to deleting '$nugetOutputPath' folder's content."
		}
	}

	foreach ($info in Get-ProjectsInfo "$srcDir" "$commonAssemblyInfoPath") {
		if (Test-Path $info.BinDirectory) {
			try {
				Write-Host "Cleaning up '$($info.BinDirectory)'."
				Remove-Item $info.BinDirectory -Force -Recurse -ErrorAction SilentlyContinue
			}
			catch {
				Exit-Build "Failed to deleting '$($info.BinDirectory)' folder."
			}
		}

		if (Test-Path $info.ObjDirectory) {
			try {
				Write-Host "Cleaning up '$($info.ObjDirectory)'."
				Remove-Item $info.ObjDirectory -Force -Recurse -ErrorAction SilentlyContinue
			}
			catch {
				Exit-Build "Failed to deleting '$($info.ObjDirectory)' folder."
			}
		}
	}
}

Task NuGet-Restore {
	$solutionPackagesFilePath = "$srcDir\.nuget\packages.config"
	
	if (Test-Path $solutionPackagesFilePath) {
		try {
			& $("$srcDir\.nuget\NuGet.exe") install "$solutionPackagesFilePath" -o "$nugetPackagesDirectoryPath"
		}
		catch {
			Exit-Build "Failed to download packages for project '$solutionPackagesFilePath'."
		}
	}

    foreach ($info in Get-ProjectsInfo "$srcDir" "$commonAssemblyInfoPath") {
        if (Test-Path $info.PackagesConfigPath) {
			Write-Host "Downloading packages for project '$($info.ProjectFile)'."

			try {
				& $("$srcDir\.nuget\NuGet.exe") install $info.PackagesConfigPath -o "$nugetPackagesDirectoryPath"
			}
			catch {
				Exit-Build "Failed to download packages for project '$($info.ProjectFile)'."
			}
			
			Write-Host ""
        }
    }
}

Task Compile {
    foreach ($info in Get-ProjectsInfo "$srcDir" "$commonAssemblyInfoPath") {
        try {
            exec { msbuild /nologo /v:$msbuildVerbosity /m:$msbuildCpuCount /p:BuildInParralel=$msbuildParralel /p:Configuration="$config" /p:Platform=$platform /p:OutDir="$($info.BinDirectory)\$config" "$($info.ProjectFilePath)" }
        }
        catch {
            Exit-Build "Failed to build project '$($info.ProjectName)'."
        }
    }
}

Task Run-UnitTests {
    $tools = @("xunit.console.exe", "xunit.console.x86.exe")

	foreach ($info in Get-ProjectsInfo "$srcDir" "$commonAssemblyInfoPath") {
		Write-Host "Examining project '$($info.ProjectFile)'."
		
		if ($info.IsTestProject) {
			$binFolder = "$($info.BinDirectory)$config"
            $assemblyPath = "$binFolder\$($info.AssemblyName).dll"
 			Write-Host "Found test assembly '$assemblyPath'." -ForegroundColor Cyan
			
			foreach ($tool in $tools) {
				$d = Get-ChildItem $baseDir -recurse | Where-Object {$_.PSIsContainer -eq $true -and $_.Name.StartsWith("xunit.runner.console")}
				$toolPath = "$($d.FullName)\tools\$tool"
				Write-Host $p
				$output = "$(& $toolPath $assemblyPath)"
				Write-Host $output
				
				if ($LastExitCode) {
					Exit-Build "One or more tests failed for project '$assemblyPath'."
				}
			}
		}
	}
}

Task NuGet-CreatePackages {
	if (-not(Test-Path $nugetOutputPath)) {
		md $nugetOutputPath
	}

	foreach ($info in Get-ProjectsInfo "$srcDir" "$commonAssemblyInfoPath") {
		if (Test-Path -Path $info.NuspecFile) {
			$version = Get-InformationalVersion "$productVersion" "$patchVersion" "$buildNumber" "$preRelease"
			
			Write-Host "Generating NuGet package for $($info.ProjectFile) from nuspec file '$($info.NuspecFile)'."
			exec { & $("$srcDir\.nuget\NuGet.exe") pack $info.ProjectFilePath -IncludeReferencedProjects -OutputDirectory $nugetOutputPath -Version $version -Prop Configuration=$config }
			
			Write-Host ""
		}
	}
}

Task NuGet-Publish {
	Write-Host "Publishing packages."
	
	foreach ($package in ls $nugetOutputPath "*.nupkg") {
		Write-Host "Publishing package $($package.FullName) to NuGet repository on '$nugetServer'."
		exec { & $(Get-Tool "NuGet" "$toolsDirectoryPath") push "$($package.FullName)" $nugetApiKey -Source $nugetServer }
	}
}

Task AssemblyInfo-Generate {
	Write-Output "Generating assemblies info."
	
	$assemblyFileVersion = Get-FileVersion "$productVersion" "$patchVersion" "$buildNumber"
	$assemblyInformationalVersion = Get-InformationalVersion "$productVersion" "$patchVersion" "$buildNumber" "$preRelease"
	$solutionInfo = Get-SolutionInfo $commonAssemblyInfoPath
	
	Generate-CommonAssemblyInfo "$($solutionInfo.CLSCompliant)" `
							    "$config" `
							    "$($solutionInfo.Company)" `
							    "$($solutionInfo.Product)" `
							    "$($solutionInfo.Copyright)" `
							    $assemblyFileVersion `
							    $assemblyFileVersion `
							    $assemblyInformationalVersion `
								"$($solutionInfo.ComVisible)" `
								"$($solutionInfo.NeutralResourcesLanguage)" `
								"$($solutionInfo.DelaySign)" `
							    $commonAssemblyInfoPath
	
	foreach ($info in Get-ProjectsInfo "$baseDir" "$commonAssemblyInfoPath") {
		Write-Host "$($info.Title)"
	
		$assemblyTitle = "$($info.Title)"
		$guid = "$($info.Guid)"
		$description = "$($info.Description)"
		$internalsVisibleTo = "$($info.InternalsVisibleTo)"
		
		Generate-AssemblyInfo $assemblyTitle $guid $description $internalsVisibleTo $($info.AssemblyInfoPath)
	}
}