#
# build.ps1
#

#Requires -Version 3.0

<#
.SYNOPSIS
    Utility script for invoking psake builds.

.DESCRIPTION
    The purpose of this utility script is to eliminate the need to commit/checkin the psake module into version control system (VCS).

    Since the psake module and other dependencies will be downloaded via NuGet, only this script and your build file need to be committed into VCS. By default only psake is specified in "packages.config" but other build dependencies can be included and used as well.

    After psake is restored via NuGet and imported, this script will then invoke psake with the supplied arguments.

.PARAMETER BuildFile
    An optional parameter specifies the path to your psake build script. If omitted, psake will use its default value of "Default.ps1".

.PARAMETER TaskList
    An optional parameter specifies the list of task names (aka targets) to execute from your psake build script. If omitted, psake will use its default value of "Default".

.PARAMETER Framework
    An optional parameter that specifies the version of the .NET framework you want to use during build. You can append x86 or x64 to force a specific framework. If not specified, x86 or x64 will be detected based on the bitness of the PowerShell process. Possible values: '1.0', '1.1', '2.0', '2.0x86', '2.0x64', '3.0', '3.0x86', '3.0x64', '3.5', '3.5x86', '3.5x64', '4.0', '4.0x86', '4.0x64', '4.5', '4.5x86', '4.5x64', '4.5.1', '4.5.1x86', '4.5.1x64'

.PARAMETER Docs
    An optional paramater that instructs psake to print a list of tasks and their descriptions.

.PARAMETER Parameters
    An optional hashtable that contains any additional parameters to be passed into the psake build script. See psake documentation for more information.

.PARAMETER Properties
    An optional hashtable that contains any additional properties to be passed into the psake build script. See psake documentation for more information.

.PARAMETER Initialization
    An optional paramater that contains an additional initialization script to invoke before the psake tasks.

.PARAMETER NoLogo
    An optional paramater that instructs psake to not display the startup banner and copyright message.

.PARAMETER DetailedDocs
    An optional paramater that instructs psake to print a detailed table of tasks and their descriptions.

.EXAMPLE
    .\build.ps1
    This example invokes the default psake script which should be named "Default.ps1" using the default task.

.EXAMPLE
    .\build.ps1 -task Clean
    This example invokes the default psake script with the 'Clean' task.

.EXAMPLE
    .\build.ps1 -script MyBuild.ps1 -task Compile -parameters @{param1="value1"; param2="value2"}
    This example invokes the specified script with the 'Compile' task and additional parameters for the script.

.LINK
    https://github.com/psake/psake/wiki
#>
[CmdletBinding()]
Param (
    [Parameter(Position = 0, Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [Alias("file")]
    [Alias("script")]
    [String] $buildFile,

    [Parameter(Position = 1, Mandatory = $false)]
    [ValidateNotNull()]
    [Alias("task")]
    [Alias("target")]
    [String[]] $taskList = @(""),

    [Parameter(Position = 2, Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [String] $framework,

    [Parameter(Position = 3, Mandatory = $false)]
    [Switch] $docs = $false,

    [Parameter(Position = 4, Mandatory = $false)]
    [ValidateNotNull()]
    [Alias("params")]
    [Hashtable] $parameters = @{},

    [Parameter(Position = 5, Mandatory = $false)]
    [ValidateNotNull()]
    [Alias("props")]
    [Hashtable] $properties = @{},

    [Parameter(Position = 6, Mandatory = $false)]
    [ValidateNotNull()]
    [Alias("init")]
    [Scriptblock] $initialization = {},

    [Parameter(Position = 7, Mandatory = $false)]
    [switch] $nologo = $false,

    [Parameter(Position = 8, Mandatory = $false)]
    [switch] $detailedDocs = $false
)
$ErrorActionPreference = "Stop"

pushd $PSScriptRoot
$PreviousDirectory = [Environment]::CurrentDirectory
[Environment]::CurrentDirectory = $PSScriptRoot
Write-Host "PSScriptRoot: $PSScriptRoot" -ForegroundColor Yellow
Write-Host "BuildFile: $buildFile" -ForegroundColor Yellow
Write-Host "TaskList: $taskList" -ForegroundColor Yellow

# don't wrap console output
$uiBufferSize = $host.UI.RawUI.BufferSize
$host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.Size(8192,50)

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
    # restore any packages via NuGet
    NuGet restore "$PSScriptRoot\packages.config" -Verbosity detailed -NonInteractive -PackagesDirectory "$PSScriptRoot\packages\"

    # find and import the psake module
    $file = Get-ChildItem -Path "$PSScriptRoot\packages" -Include "psake.psm1" -Recurse | Sort -Descending | Select-Object -First 1
    if (!$file) {
        throw ("Unable to find the psake module.")
    }
    Import-Module -Name $file.FullName

    try {
        # invoke psake with argument spatting
        $argsHash = @{}
        if ($buildFile) {
            $argsHash.buildFile = $buildFile;
        }
        if ($taskList) {
            $argsHash.taskList = $taskList;
        }
        if ($framework) {
            $argsHash.framework = $framework;
        }
        if ($docs) {
            $argsHash.docs = $docs;
        }
        if ($parameters) {
            $argsHash.parameters = $parameters;
        }
        if ($properties) {
            $argsHash.properties = $properties;
        }
        if ($initialization) {
            $argsHash.initialization = $initialization;
        }
        if ($nologo) {
            $argsHash.nologo = $nologo;
        }
        if ($detailedDocs) {
            $argsHash.detailedDocs = $detailedDocs;
        }
        Invoke-psake @argsHash
    }
    finally {
        Remove-Module -Name "psake" -Force
    }
}
finally {
    $host.UI.RawUI.BufferSize = $uiBufferSize
    [Environment]::CurrentDirectory = $PreviousDirectory
    popd
}
