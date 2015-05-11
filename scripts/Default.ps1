
if ($psake -eq $null) {
    Write-Host "Using Bootstrap" -ForegroundColor Yellow
    . $PSScriptRoot\Build.ps1 -buildFile $PSCommandPath @args
    return
}

Framework "4.0"

Properties {
    $scriptDir = $psake.build_script_dir
    $baseDir = Split-Path $scriptDir
    $sourceDir = Join-Path $baseDir "src"
    $outputDir = Join-Path $baseDir "build"
    $testDir = Join-Path $baseDir "TestResults"

    # http://social.technet.microsoft.com/wiki/contents/articles/7804.powershell-creating-custom-objects.aspx
    $build = [PSCustomObject] @{
        name = NullIf ${build.name} "NCode.Scanners"
        configuration = NullIf ${build.configuration} "Release"
        platform = NullIf ${build.platform} "Any CPU"
        input = NullIf ${build.input} "$sourceDir\$name.sln"
    }

    $buildName = NullIf ${build.name} "NCode.Scanners"
    $configurationName = NullIf ${build.configuration} "Release"
    $platformName = NullIf ${build.platform} "Any CPU"

    $solutionName = NullIf ${build.input} "$buildName.sln"
    $solutionPath = Join-Path $sourceDir $solutionName

    $nunit = @{
        runner = ${nunit.runner}
        x86 = if (${nunit.x86} -ne $null) { ${nunit.x86} -eq $true } else { $false }
        nologo = if (${nunit.nologo} -ne $null) { ${nunit.nologo} -eq $true } else { $true }
        framework = NullIf ${nunit.framework} "4.0"
        apartment = ${nunit.apartment}
        work = CombineBase $baseDir ${nunit.work} "TestResults"
        result = ${nunit.result}
        out = ${nunit.out}
        err = ${nunit.err}
        include = ${nunit.include}
        exclude = ${nunit.exclude}
        process = ${nunit.process}
        domain = ${nunit.domain}
    }

    $msbuild = [PSCustomObject] @{
        arguments = [string[]] @()
        properties = @{}
        verbosity = "minimal"
    }

    $isRunningInTeamCity = ${env:teamcity.dotnet.nunitaddin} -ne $null
}

function NullIf([String] $value1, [String] $value2) {
    if ([String]::IsNullOrEmpty($value1)) {
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

FormatTaskName (("-"*25) + "[{0}]" + ("-"*25))

Task Default -Depends Test

Task Init {
    New-Item $outputDir -ItemType Directory -Force | Out-Null
    New-Item $testDir -ItemType Directory -Force | Out-Null
}

Task Purge {
    Get-ChildItem $outputDir | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
}

Task Restore {
    NuGet restore "$solutionPath"
}

Task Clean -depends Init {
    Invoke-MSBuild -nologo -verbosity minimal -maxcpucount `
        -file $solutionPath `
        -targets "Clean" `
        -configuration $configurationName `
        -outputDir $outputDir `
        -properties @{GenerateProjectSpecificOutputFolder=$true}
}

Task Compile -depends Init, Restore, Clean {
    Invoke-MSBuild -nologo -verbosity minimal -maxcpucount `
        -file $solutionPath `
        -targets "Build" `
        -configuration $configurationName `
        -platform $platformName `
        -outputDir $outputDir `
        -properties @{GenerateProjectSpecificOutputFolder=$true}
}

Task NUnit-Probe -depends Restore {
    if ([String]::IsNullOrEmpty($nunit.runner) -or !(Test-Path $nunit.runner)) {
        [String] $platform = ""
        if ($nunit.x86 -eq $true) {
            $platform = "-x86"
        }
        $nunit.runner = Get-ChildItem -Path $sourceDir -Include "nunit-console$platform.exe" -Recurse | Sort-Object | Select-Object -Last 1
    }
}

Task NUnit-Addin -depends NUnit-Probe -precondition { return $isRunningInTeamCity } {
    $name = [System.Reflection.AssemblyName]::GetAssemblyName($nunit.runner)
    $version = $name.Version.ToString(3)

    $base = Split-Path $nunit.runner
    $addinsDir = Join-Path $base addins
    New-Item $addinsDir -Type Directory -Force | Out-Null

    $teamCityDir = "${env:teamcity.dotnet.nunitaddin}-$version.*"
    Copy-Item $teamCityDir -Destination $addinsDir
}

Task Test -depends NUnit-Probe, NUnit-Addin, Compile, Clean {
    $resultsDir = Join-Path $baseDir "TestResults"
    New-Item $resultsDir -Type Directory -Force | Out-Null
    $tests = Get-ChildItem -Path $outputDir -Recurse -Include *Test*.dll

    [Hashtable] $argsHash = @{}
    $knownArgs = (Get-Command Invoke-NUnit).Parameters
    foreach ($kvp in $nunit.GetEnumerator()) {
        $key = $kvp.Key
        $value = $kvp.Value
        if ($key -ne $null -and $value -ne $null -and $knownArgs.ContainsKey($key)) {
            $argsHash[$key] = $value
        }
    }
    $argsHash.tests = $tests
    Invoke-NUnit @argsHash
}

function Invoke-NUnit {
    # http://www.nunit.org/index.php?p=consoleCommandLine&r=2.6.4
    [CmdletBinding()]
    param (
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
        [Switch] $cleanup
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

    #
    # Quoting is not necessary when using arrays. See the following:
    # http://edgylogic.com/blog/powershell-and-external-commands-done-right/
    #

    Write-Host "`"$runner`" $argsArray" -ForegroundColor Yellow
    exec { & "$runner" $argsArray }
}

function Invoke-MSBuild {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [Alias("f")]
        [String] $file,

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
        [String[]] $arguments = @()
    )

    [String[]] $argsArray = @($file)

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
