#
# settings.ps1
#

Include functions.ps1

Properties {
    $build = @{}

    # override these properties using parameters in the form of '${build.foo} = bar' when invoking psake

    $build.baseDir = NullIf ${build.baseDir} (Split-Path $psake.build_script_dir)
    $build.sourceDir = CombineBase $build.baseDir ${build.sourceDir} "src"
    $build.outputDir = CombineBase $build.baseDir ${build.outputDir} "build"
    $build.testDir = CombineBase $build.baseDir ${build.testDir} "test-results"

    $build.name = NullIf ${build.name} (Split-Path $build.baseDir -Leaf)
    $build.configuration = NullIf ${build.configuration} "Release"
    $build.platform = NullIf ${build.platform} "Any CPU"

    $build.projectExt = NullIf ${build.projectExt} ".sln"
    $build.project = CombineBase $build.sourceDir ${build.project} "$($build.name)$($build.projectExt)"
    $build.generateProjectSpecificOutputFolder = NullIf ${build.generateProjectSpecificOutputFolder} $true

    # register our settings so that they can be shown with the 'ShowSettings' task
    $build.settings = @(
        @{ prefix="build"; settings=$build }
    )
}

Task ShowSettings {
    # combine all the settings into a single hastable
    $all = @{}
    $build.settings |% {
        $prefix = $_.prefix
        $settings = $_.settings

        $settings.GetEnumerator() |% {
            $all["$prefix." + $_.Key] = $_.Value
        }
    }
    # then display all the settigs
    $all.GetEnumerator() |
        Sort-Object -Property Name |
        Format-Table -AutoSize -Property Name, @{n='Value';e={
            # expand any inner hastables or scriptblocks
            if ($_.Value -is [Hashtable]) {
                $inner = $_.Value
                $pairs = $inner.Keys | Sort | %{ "{0}={1}" -f $_, $inner[$_] }
                "{ $($pairs -join '; ') }"
            } elseif ($_.Value -is [Scriptblock]) {
                "{$($_.Value)}"
            } else {
                $_.Value
            }
        }} |
        Out-String |
        Write-Host -ForegroundColor Yellow
}
