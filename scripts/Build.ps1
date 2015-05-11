#Requires -Version 3.0

#
# Set-ExecutionPolicy RemoteSigned
#
# powershell -NoProfile -ExecutionPolicy Bypass -File .\script.ps1
#

[CmdletBinding()]
param (
    [Parameter(Position = 0, Mandatory = $false)]
    [String] $buildFile,

    [Parameter(Position = 1, Mandatory = $false)]
    [String[]] $taskList = @(""),

    [Parameter(Mandatory = $false)]
    [Hashtable] $parameters = @{},

    [Parameter(Mandatory = $false)]
    [Hashtable] $properties = @{}
)
$ErrorActionPreference = "Stop"

pushd $PSScriptRoot
$PreviousDirectory = [Environment]::CurrentDirectory
[Environment]::CurrentDirectory = $PSScriptRoot
Write-Host "PSScriptRoot: $PSScriptRoot"

try {
    # first make sure NuGet exists
    try {
        & NuGet help | Out-Null
    }
    catch [System.Management.Automation.CommandNotFoundException] {
        $sourceNuGetExe = "https://NuGet.org/NuGet.exe"
        $targetNuGetExe = Join-Path -Path $PSScriptRoot -ChildPath "NuGet.exe"
        Invoke-WebRequest $sourceNuGetExe -OutFile $targetNuGetExe -Verbose
        Set-Alias NuGet $targetNuGetExe -Scope Global -Verbose
    }
    # tell NuGet to auto-update itself
    & NuGet update -self
    if ($lastexitcode -ne 0) {
        throw ("Error executing command 'NuGet update -self'.")
    }
    # restore psake package using NuGet
    NuGet restore "$PSScriptRoot\nuget\packages.config" -Verbosity detailed -NonInteractive -PackagesDirectory "$PSScriptRoot\nuget\packages\"

    $file = Get-ChildItem -Path "$PSScriptRoot\nuget\packages" -Include "psake.psm1" -Recurse | Select-Object -First 1
    Import-Module -Name $file.FullName
    try {
        Invoke-psake -buildFile $buildFile -taskList $taskList -parameters $parameters -properties $properties
    }
    finally {
        Remove-Module -Name "psake" -Force
    }
}
finally {
    [Environment]::CurrentDirectory = $PreviousDirectory
    popd
}
