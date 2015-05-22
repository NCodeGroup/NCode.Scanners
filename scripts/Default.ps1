
if ($psake -eq $null) {
    cls
    Write-Host "Using Bootstrap" -ForegroundColor Yellow
    . $PSScriptRoot\Build.ps1 -buildFile $PSCommandPath @args
    return
}

# Framework "4.0"

Properties {
    $build = @{}

    $build.baseDir = NullIf ${build.baseDir} (Split-Path $psake.build_script_dir)
    $build.sourceDir = CombineBase $build.baseDir ${build.sourceDir} "src"
    $build.outputDir = CombineBase $build.baseDir ${build.outputDir} "build"

    $build.name = NullIf ${build.name} (Split-Path $build.baseDir -Leaf)
    $build.configuration = NullIf ${build.configuration} "Release"
    $build.platform = NullIf ${build.platform} "Any CPU"

    $build.projectExt = NullIf ${build.projectExt} ".sln"
    $build.project = CombineBase $build.sourceDir ${build.project} "$($build.name)$($build.projectExt)"
    $build.generateProjectSpecificOutputFolder = NullIf ${build.generateProjectSpecificOutputFolder} $true

    # defaults for 'nunit'
    $nunit = @{
        x86 = $false
        nologo = $true
        work = "test-results"
        framework = $psake.context.config.framework
        tests = { Get-ChildItem -Path $build.outputDir -Recurse -Include *Test*.dll }
    }
    # bind any 'nunit' variables to properties
    Bind-Parameters -command "Invoke-NUnit" -prefix "nunit" -arguments $nunit
    # convert any relative paths to absolute paths
    $nunit.work = [System.IO.Path]::Combine($build.baseDir, $nunit.work)

    # defaults for 'msbuild'
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
        "target.clean" = "Clean"
        "target.build" = "Build"
    }
    # bind any 'msbuild' variables to properties
    Bind-Parameters -command "Invoke-MSBuild" -prefix "msbuild" -arguments $msbuild
    # convert any relative paths to absolute paths
    $msbuild.outputDir = [System.IO.Path]::Combine($build.baseDir, $msbuild.outputDir)

    # defaults for 'pack'
    $pack = @{
        Version = $build.version
        Verbosity = "detailed"
        NonInteractive = $true
        NoPackageAnalysis = $true
        BasePath = $build.baseDir
        OutputDirectory = $msbuild.outputDir
        Properties = @{
            GenerateProjectSpecificOutputFolder = $build.generateProjectSpecificOutputFolder
            Configuration = $build.configuration
            OutDir = $msbuild.outputDir
            Timestamp = [DateTimeOffset]::Now.ToString("O")
            Year = [DateTime]::Now.Year
        }
    }
    # bind any 'pack' variables to properties
    Bind-Parameters -command "Invoke-Pack" -prefix "pack" -arguments $pack
    # convert any relative paths to absolute paths
    $pack.BasePath = [System.IO.Path]::Combine($build.baseDir, $pack.BasePath)
    $pack.OutputDirectory = [System.IO.Path]::Combine($build.baseDir, $pack.OutputDirectory)

    $isRunningInTeamCity = ${env:teamcity.dotnet.nunitaddin} -ne $null
}

FormatTaskName (("-"*25) + "[{0}]" + ("-"*25))

Task Default -Depends Test

Task Info {
    $info = @{}
    $build.GetEnumerator() | %{ $info["build." + $_.Key] = $_.Value }
    $nunit.GetEnumerator() | %{ $info["nunit." + $_.Key] = $_.Value }
    $msbuild.GetEnumerator() | %{ $info["msbuild." + $_.Key] = $_.Value }
    $pack.GetEnumerator() | %{ $info["pack." + $_.Key] = $_.Value }
    $info.GetEnumerator() | Sort-Object -Property Name | Format-Table -AutoSize | Out-String | Write-Host -ForegroundColor Yellow
}

Task Init -depends Info {
    New-Item $build.outputDir -ItemType Directory -Force | Out-Null
    New-Item $nunit.work -ItemType Directory -Force | Out-Null
}

Task Purge -depends Init {
    Get-ChildItem $build.outputDir | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
    Get-ChildItem $nunit.work | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
}

Task Restore -depends Info {
    NuGet restore $build.project
}

Task Clean -depends Init {
    $msbuild = $msbuild.Clone()
    $msbuild.target = $msbuild["target.clean"]
    Invoke-MSBuild @msbuild
}

Task Compile -depends Init, Restore, Clean {
    $msbuild = $msbuild.Clone()
    $msbuild.target = $msbuild["target.build"]
    Invoke-MSBuild @msbuild
}

Task NUnit-Probe -depends Restore {
    if ([String]::IsNullOrEmpty($nunit.runner) -or !(Test-Path $nunit.runner)) {
        [String] $platform = ""
        if ($nunit.x86 -eq $true) {
            $platform = "-x86"
        }
        $nunit.runner = Get-ChildItem -Path $build.sourceDir -Include "nunit-console$platform.exe" -Recurse | Sort-Object | Select-Object -Last 1
    }
}

Task NUnit-Addin -depends NUnit-Probe -precondition { return $isRunningInTeamCity } {
    Write-Host "Initializing TeamCity addin for NUnit" -ForegroundColor Yellow

    $name = [System.Reflection.AssemblyName]::GetAssemblyName($nunit.runner)
    $version = $name.Version.ToString(3)

    $base = Split-Path $nunit.runner
    $addinsDir = Join-Path $base addins
    New-Item $addinsDir -Type Directory -Force | Out-Null

    $teamCityDir = "${env:teamcity.dotnet.nunitaddin}-$version.*"
    Copy-Item $teamCityDir -Destination $addinsDir
}

Task Test -depends NUnit-Probe, NUnit-Addin, Compile, Clean {
    $nunit = $nunit.Clone()
    if ($nunit.tests -is [Scriptblock]) {
        $nunit.tests = &$nunit.tests
    }
    Invoke-NUnit @nunit
}

Task Package -depends Test, Compile, Clean {
    Get-ChildItem -Path $build.sourceDir -Include *.nuspec -Recurse |% {
        $nuspec = $_
        $csproj = [System.IO.Path]::ChangeExtension($nuspec, ".??proj")

        $pack = $pack.Clone()
        if (Test-Path $csproj) {
            $pack.nuspec = Resolve-Path $csproj
        } else {
            $pack.nuspec = $nuspec
        }
        Invoke-Pack @pack
    }
}

function NullIf($value1, $value2) {
    if ($value1 -eq $null) {
        return $value2
    }
    return $value1
}

function CombineBase([String] $base, [String] $path, [String] $default) {
    if ([String]::IsNullOrEmpty($path)) {
        $path = $default
    }
    return [System.IO.Path]::Combine($base, $path)
}

function Bind-Parameters {
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory = $true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [String] $command,

        [Parameter(Mandatory = $true, Position = 1)]
        [ValidateNotNullOrEmpty()]
        [String] $prefix,

        [Parameter(Mandatory = $true, Position = 2)]
        [ValidateNotNull()]
        [Hashtable] $arguments
    )

    $cmd = Get-Command -Name $command
    $parameters = $cmd.Parameters.Values.GetEnumerator()
    $variables = Get-Variable -Name "$prefix*"

    foreach ($parameter in $parameters) {
        $name = $parameter.Name

        $variable = $variables |? { $_.Name -eq "$prefix.$name" }
        if ($variable) {
            $newValue = $variable.Value
            $prevValue = $arguments[$name]
            if ($newValue -is [Hashtable] -and $prevValue -is [Hashtable]) {
                foreach ($kvp in $newValue.GetEnumerator()) {
                    $prevValue[$kvp.Key] = $kvp.Value
                }
            } else {
                $arguments[$name] = $newValue
            }
        }

        $validateNotNullOrEmpty = @($parameter.Attributes |?{ [ValidateNotNullOrEmpty].IsInstanceOfType($_) }).Count -gt 0
        $validateNotNull = $validateNotNullOrEmpty -or (@($parameter.Attributes |?{ [ValidateNotNull].IsInstanceOfType($_) }).Count -gt 0)

        if ($arguments.ContainsKey($name)) {
            $value = $arguments[$name]
            if (($validateNotNull -and $value -eq $null) -or ($validateNotNullOrEmpty -and [String]::IsNullOrEmpty($value))) {
                $arguments.Remove($name)
            }
        }
    }
}

function Invoke-NUnit {
    # http://www.nunit.org/index.php?p=consoleCommandLine&r=2.6.4
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory = $true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [ValidateScript({ Test-Path $_ })]
        [String] $runner,

        [Parameter(Mandatory = $true, Position = 1)]
        [ValidateNotNullOrEmpty()]
        [String[]] $tests,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String[]] $run,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [ValidateScript({ Test-Path $_ })]
        [String] $runList,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String] $config,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String[]] $include,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String[]] $exclude,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String] $framework,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [ValidateSet("Single", "Separate", "Multiple")]
        [String] $process,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [ValidateSet("None", "Single", "Multiple")]
        [String] $domain,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [ValidateSet("STA", "MTA")]
        [String] $apartment,

        [Parameter(Mandatory = $false)]
        [Int32] $timeout,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String] $out,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String] $err,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String] $result,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [String] $work,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [ValidateSet("Off", "Error", "Warning", "Info", "Debug")]
        [String] $trace,

        [Parameter(Mandatory = $false)]
        [Switch] $noshadow,

        [Parameter(Mandatory = $false)]
        [Switch] $nothread,

        [Parameter(Mandatory = $false)]
        [Switch] $stoponerror,

        [Parameter(Mandatory = $false)]
        [Switch] $wait,

        [Parameter(Mandatory = $false)]
        [Switch] $xmlconsole,

        [Parameter(Mandatory = $false)]
        [Switch] $nologo,

        [Parameter(Mandatory = $false)]
        [Switch] $cleanup,

        [Parameter(Mandatory = $false)]
        [ValidateNotNull()]
        [Alias("args")]
        [String[]] $arguments = @(),

        # the following parameter is used to skip/ignore unknown arguments passed into this function
        [Parameter(Mandatory = $false, ValueFromRemainingArguments = $true)]
        $others
    )

    [String[]] $argsArray = @($tests)

    if ($run) {
        [String] $run = $run -join ","
        $argsArray += "/run=$run"
    }
    if ($runList) {
        $argsArray += "/runList=$runList"
    }
    if ($config) {
        $argsArray += "/config=$config"
    }
    if ($include) {
        [String] $include = $include -join ","
        $argsArray += "/include=$include"
    }
    if ($exclude) {
        [String] $exclude = $exclude -join ","
        $argsArray += "/exclude=$exclude"
    }
    if ($framework) {
        $argsArray += "/framework=$framework"
    }
    if ($process) {
        $argsArray += "/process=$process"
    }
    if ($domain) {
        $argsArray += "/domain=$domain"
    }
    if ($apartment) {
        $argsArray += "/apartment=$apartment"
    }
    if ($timeout -gt 0) {
        $argsArray += "/timeout=$timeout"
    }
    if ($out) {
        $argsArray += "/out=$out"
    }
    if ($err) {
        $argsArray += "/err=$err"
    }
    if ($result) {
        $argsArray += "/result=$result"
    }
    if ($work) {
        $argsArray += "/work=$work"
    }
    if ($trace) {
        $argsArray += "/trace=$trace"
    }
    if ($noshadow) {
        $argsArray += "/noshadow"
    }
    if ($nothread) {
        $argsArray += "/nothread"
    }
    if ($stoponerror) {
        $argsArray += "/stoponerror"
    }
    if ($wait) {
        $argsArray += "/wait"
    }
    if ($xmlconsole) {
        $argsArray += "/xmlconsole"
    }
    if ($nologo) {
        $argsArray += "/nologo"
    }
    if ($cleanup) {
        $argsArray += "/cleanup"
    }
    foreach ($argument in $arguments) {
        $argsArray += $argument
    }

    #
    # Quoting is not necessary when using arrays. See the following:
    # http://edgylogic.com/blog/powershell-and-external-commands-done-right/
    #

    Write-Host "`"$runner`" $argsArray" -ForegroundColor Yellow
    exec { & "$runner" $argsArray }
}

function Invoke-MSBuild {
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory = $true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [Alias("solution")]
        [String] $project,

        [Parameter(Mandatory = $true, Position = 1)]
        [ValidateNotNullOrEmpty()]
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
