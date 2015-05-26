#
# default.ps1
#

<#
.SYNOPSIS
    A generic build file for psake that doesn't refer to any project specific details.
#>

# check if we are in the psake context
if ($psake -eq $null) {
    # if not, then use the utility script to invoke psake on ourself
    Write-Host "Using Bootstrap" -ForegroundColor Yellow
    . $PSScriptRoot\Build.ps1 -buildFile $PSCommandPath @args
    return
}

Include functions.ps1
Include settings.ps1
Include msbuild.ps1
Include nunit.ps1
Include pack.ps1

FormatTaskName (("-"*25) + "[{0}]" + ("-"*25))

Task Default -Depends Test

Task Init -depends ShowSettings {
    New-Item $build.outputDir -ItemType Directory -Force | Out-Null
    New-Item $build.testDir -ItemType Directory -Force | Out-Null
}

Task Purge -depends Init {
    Get-ChildItem $build.outputDir | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
    Get-ChildItem $nunit.work | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
}

Task Restore -depends Init {
    NuGet restore $build.project
}

Task Clean -depends Init {
    Invoke-MSBuild @msbuild
}

Task Compile -depends Init, Restore, Clean {
    Invoke-MSBuild @msbuild
}

Task Test -depends NUnit-Test, Compile, Clean {
}

Task Pack -depends NuGet-Pack, Test, Compile, Clean {
}
