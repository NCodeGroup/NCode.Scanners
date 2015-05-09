#Requires -Version 3.0

#
# Set-ExecutionPolicy RemoteSigned
#
# powershell -NoProfile -ExecutionPolicy Bypass -File .\script.ps1
#

Write-Host "PSScriptRoot: $PSScriptRoot"

# $rootPath = "C:\Users\polewskm\Documents\Save\Code"
# Set-Location -Path $rootPath

try {
    & NuGet help | Out-Null
}
catch [System.Management.Automation.CommandNotFoundException] {
    $sourceNuGetExe = "http://NuGet.org/NuGet.exe"
    $targetNuGetExe = Join-Path -Path $PSScriptRoot -ChildPath "NuGet.exe"
    Invoke-WebRequest $sourceNuGetExe -OutFile $targetNuGetExe -Verbose
    Set-Alias NuGet $targetNuGetExe -Scope Global -Verbose
}
& NuGet update -self
if ($lastexitcode -ne 0) {
    throw ("Error executing command 'NuGet update -self'.")
}
NuGet restore .\psake\packages.config -Verbosity detailed -NonInteractive -PackagesDirectory .\psake\packages\

$file = Get-ChildItem -Path ".\psake\packages" -Include "psake.psm1" -Recurse | Select-Object -First 1
Import-Module -Name $file.FullName
try {
}
finally {
    Remove-Module -Name "psake" -Force
}
