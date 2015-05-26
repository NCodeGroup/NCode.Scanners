#
# nunit.ps1
#

Include functions.ps1

Properties {

    # defaults for 'nunit'
    # override these properties using parameters in the form of '${nunit.foo} = bar'
    $nunit = @{
        x86 = $false
        nologo = $true
        work = $build.testDir
        framework = $psake.context.config.framework
        tests = { Get-ChildItem -Path $build.outputDir -Include *Test*.dll -Recurse }
        teamcity = ${env:teamcity.dotnet.nunitaddin}
    }

    # bind the 'nunit' variables to properties using the signature from 'Invoke-NUnit'
    Bind-Parameters -command "Invoke-NUnit" -prefix "nunit" -arguments $nunit

    # register our settings so that they can be shown with the 'ShowSettings' task
    $build.settings += @{ prefix="nunit"; settings=$nunit }
}

Task NUnit-Probe -depends Restore {
    if ([String]::IsNullOrEmpty($nunit.runner) -or !(Test-Path $nunit.runner)) {
        [String] $platform = ""
        if ($nunit.x86 -eq $true) {
            $platform = "-x86"
        }
        $nunit.runner = Get-ChildItem -Path $build.sourceDir -Include "nunit-console$platform.exe" -Recurse |
            Select-Object -ExpandProperty VersionInfo |
            Sort-Object -Descending -Property ProductVersion |
            Select-Object -Last 1 -ExpandProperty FileName
        if (!$nunit.runner) {
            throw ("Unable to find NUnit test runner.")
        }
    }
}

Task NUnit-Addin -depends NUnit-Probe -precondition { return ![String]::IsNullOrEmpty($nunit.teamcity) } {
    Write-Host "Initializing TeamCity addin for NUnit" -ForegroundColor Yellow

    $nunitName = [System.Reflection.AssemblyName]::GetAssemblyName($nunit.runner)
    $nunitVersion = $nunitName.Version.ToString(3)

    $addinsBase = Split-Path $nunit.runner
    $addinsTarget = Join-Path $addinsBase addins
    New-Item $addinsTarget -Type Directory -Force | Out-Null

    $addinName = $nunit.teamcity
    $addinsSource = "$addinName-$nunitVersion.*"
    Copy-Item $addinsSource -Destination $addinsTarget
}

Task NUnit-Test -depends NUnit-Probe, NUnit-Addin, Compile, Clean {
    $nunit = $nunit.Clone()
    if ($nunit.tests -is [Scriptblock]) {
        $nunit.tests = &$nunit.tests
    }
    Invoke-NUnit @nunit
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
