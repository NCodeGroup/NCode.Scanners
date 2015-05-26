#
# msbuild.ps1
#

Include functions.ps1

Properties {

    # defaults for 'msbuild'
    # override these properties using parameters in the form of '${msbuild.foo} = bar'
    $msbuild = @{
        nologo = $true
        verbosity = "minimal"
        maxcpucount = $true
        project = $build.project
        configuration = $build.configuration
        outputDir = $build.outputDir
        properties = @{
            GenerateProjectSpecificOutputFolder = $build.generateProjectSpecificOutputFolder
        }
        "target.Clean" = "Clean"
        "target.Compile" = "Build"
    }

    # bind the 'msbuild' variables to properties using the signature from 'Invoke-MSBuild'
    Bind-Parameters -command "Invoke-MSBuild" -prefix "msbuild" -arguments $msbuild

    # register our settings so that they can be shown with the 'ShowSettings' task
    $build.settings += @{ prefix="msbuild"; settings=$msbuild }
}

function Invoke-MSBuild {
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory = $true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [Alias("solution")]
        [String] $project,

        [Parameter(Mandatory = $false, Position = 1)]
        [ValidateNotNull()]
        [Alias("t", "target")]
        [String[]] $targets,

        [Parameter(Mandatory = $false, Position = 2)]
        [ValidateNotNullOrEmpty()]
        [Alias("c", "config")]
        [String] $configuration,

        [Parameter(Mandatory = $false, Position = 3)]
        [ValidateNotNullOrEmpty()]
        [String] $platform,

        [Parameter(Mandatory = $false, Position = 4)]
        [ValidateNotNullOrEmpty()]
        [Alias("o")]
        [String] $outputDir,

        [Parameter(Mandatory = $false, Position = 5)]
        [ValidateNotNull()]
        [Alias("p")]
        [Hashtable] $properties = @{},

        [Parameter(Mandatory = $false)]
        [Alias("m")]
        [Switch] $maxcpucount,

        [Parameter(Mandatory = $false)]
        [ValidateNotNull()]
        [ValidateRange(1, [Int32]::MaxValue)]
        [Alias("n")]
        [Nullable[Int]] $cpucount,

        [Parameter(Mandatory = $false)]
        [Switch] $nologo,

        [Parameter(Mandatory = $false)]
        [ValidateSet("quiet", "minimal", "normal", "detailed", "diagnostic")]
        [ValidateNotNullOrEmpty()]
        [Alias("v")]
        [String] $verbosity = "quiet",

        [Parameter(Mandatory = $false)]
        [ValidateNotNull()]
        [Alias("args")]
        [String[]] $arguments = @(),

        # the following parameter is used to skip/ignore unknown arguments passed into this function
        [Parameter(Mandatory = $false, ValueFromRemainingArguments = $true)]
        $others
    )

    # if targets is null or empty, then attempt to determine from the current psake task
    if ((!$targets -or $targets.Length -eq 0) -and $psake) {
        $context = $psake.context.Peek()
        $target = $context.currentTaskName
        $lookup = "target.{0}" -f $target
        if ($msbuild.ContainsKey($lookup)) {
            $target = $msbuild[$lookup]
        }
        $targets = @($target)
    }

    [String[]] $argsArray = @($project)

    if ($nologo -eq $true) {
        $argsArray += "/nologo"
    }
    if ($verbosity -ne $null) {
        $argsArray += "/v:$verbosity"
    }
    foreach ($target in $targets) {
        $argsArray += "/t:$target"
    }
    if ($maxcpucount -eq $true) {
        $argsArray += "/m"
    } elseif ($cpucount -ne $null) {
        $argsArray += "/m:$cpucount"
    }
    if (![String]::IsNullOrEmpty($configuration)) {
        $properties["Configuration"] = $configuration
    }
    if (![String]::IsNullOrEmpty($platform)) {
        $properties["Platform"] = $platform
    }
    if (![String]::IsNullOrEmpty($outputDir)) {
        $properties["OutDir"] = $outputDir
    }
    foreach ($property in $properties.GetEnumerator()) {
        [String] $key = $property.Key
        [String] $value = $property.Value
        $argsArray += "/p:$key=$value"
    }
    foreach ($argument in $arguments) {
        $argsArray += $argument
    }

    #
    # Quoting is not necessary when using arrays. See the following:
    # http://edgylogic.com/blog/powershell-and-external-commands-done-right/
    #

    Write-Host "msbuild $argsArray" -ForegroundColor Yellow
    exec { msbuild $argsArray }
}
