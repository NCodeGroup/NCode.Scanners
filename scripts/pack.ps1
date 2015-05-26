#
# pack.ps1
#

Include functions.ps1

Properties {

    # defaults for 'pack'
    # override these properties using parameters in the form of '${pack.foo} = bar'
    $pack = @{
        UseProjectFile = $true
        Version = $build.version
        Verbosity = "detailed"
        NonInteractive = $true
        NoPackageAnalysis = $true
        BasePath = $build.baseDir
        OutputDirectory = $build.outputDir
        Properties = @{
            GenerateProjectSpecificOutputFolder = $build.generateProjectSpecificOutputFolder
            Configuration = $build.configuration
            OutDir = $build.outputDir
            Timestamp = [DateTimeOffset]::Now.ToString("O")
            Year = [DateTime]::Now.Year
        }
        NuSpec = { Get-ChildItem -Path $build.sourceDir -Include *.nuspec -Recurse }
    }

    # bind the 'pack' variables to properties using the signature from 'Invoke-Pack'
    Bind-Parameters -command "Invoke-Pack" -prefix "pack" -arguments $pack

    # register our settings so that they can be shown with the 'ShowSettings' task
    $build.settings += @{ prefix="pack"; settings=$pack }
}

Task NuGet-Pack -depends Test, Compile, Clean {
    $nuspecList = $pack.NuSpec
    if ($nuspecList -is [Scriptblock]) {
        $nuspecList = &$nuspecList
    }
    @($nuspecList) |% {
        $nuspec = $_
        $pack = $pack.Clone()
        $projectFile = [System.IO.Path]::ChangeExtension($nuspec, ".??proj")
        if ($pack.UseProjectFile -and (Test-Path $projectFile)) {
            # we must resolve the path to remove the '?' characters
            $pack.nuspec = Resolve-Path $projectFile
        } else {
            $pack.nuspec = $nuspec
        }
        Invoke-Pack @pack
    }
}

function Invoke-Pack {
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory = $true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [String] $nuspec,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [Alias("ver")]
        [String] $version,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [Alias("base")]
        [String] $basePath,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [Alias("out")]
        [String] $outputDirectory,

        [Parameter(Mandatory = $false)]
        [ValidateNotNull()]
        [Alias("props")]
        [Hashtable] $properties = @{},

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String] $exclude,

        [Parameter(Mandatory = $false)]
        [Switch] $tool,

        [Parameter(Mandatory = $false)]
        [Switch] $build,

        [Parameter(Mandatory = $false)]
        [Switch] $symbols,

        [Parameter(Mandatory = $false)]
        [Switch] $noDefaultExcludes,

        [Parameter(Mandatory = $false)]
        [Switch] $noPackageAnalysis,

        [Parameter(Mandatory = $false)]
        [Switch] $includeReferencedProjects,

        [Parameter(Mandatory = $false)]
        [Switch] $excludeEmptyDirectories,

        [Parameter(Mandatory = $false)]
        [Switch] $nonInteractive = $true,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String] $minClientVersion,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [ValidateSet("quiet", "normal", "detailed")]
        [String] $verbosity = "detailed",

        [Parameter(Mandatory = $false)]
        [ValidateNotNull()]
        [Alias("args")]
        [String[]] $arguments = @(),

        # the following parameter is used to skip/ignore unknown arguments passed into this function
        [Parameter(Mandatory = $false, ValueFromRemainingArguments = $true)]
        $others
    )

    [String[]] $argsArray = @("pack", $nuspec)

    if ($tool) {
        $argsArray+= "-Tool"
    }
    if ($build) {
        $argsArray+= "-Build"
    }
    if ($symbols) {
        $argsArray+= "-Symbols"
    }
    if ($noDefaultExcludes) {
        $argsArray+= "-NoDefaultExcludes"
    }
    if ($noPackageAnalysis) {
        $argsArray+= "-NoPackageAnalysis"
    }
    if ($includeReferencedProjects) {
        $argsArray+= "-IncludeReferencedProjects"
    }
    if ($excludeEmptyDirectories) {
        $argsArray+= "-ExcludeEmptyDirectories"
    }
    if ($nonInteractive) {
        $argsArray+= "-NonInteractive"
    }
    if (![String]::IsNullOrEmpty($verbosity)) {
        $argsArray+= "-Verbosity"
        $argsArray+= $verbosity
    }
    if (![String]::IsNullOrEmpty($version)) {
        $argsArray+= "-Version"
        $argsArray+= $version
    }
    if (![String]::IsNullOrEmpty($basePath)) {
        $argsArray+= "-BasePath"
        $argsArray+= $basePath
    }
    if (![String]::IsNullOrEmpty($outputDirectory)) {
        $argsArray+= "-OutputDirectory"
        $argsArray+= $outputDirectory
    }
    foreach ($property in $properties.GetEnumerator()) {
        [String] $key = $property.Key
        [String] $value = $property.Value
        $argsArray += "-Prop"
        $argsArray += "$key=$value"
    }
    if (![String]::IsNullOrEmpty($exclude)) {
        $argsArray+= "-Exclude"
        $argsArray+= $exclude
    }
    if (![String]::IsNullOrEmpty($minClientVersion)) {
        $argsArray+= "-MinClientVersion"
        $argsArray+= $minClientVersion
    }
    foreach ($argument in $arguments) {
        $argsArray += $argument
    }

    #
    # Quoting is not necessary when using arrays. See the following:
    # http://edgylogic.com/blog/powershell-and-external-commands-done-right/
    #

    Write-Host "NuGet $argsArray" -ForegroundColor Yellow
    exec { NuGet $argsArray }
}
