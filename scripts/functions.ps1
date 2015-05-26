#
# functions.ps1
#
$ErrorActionPreference = "Stop"

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

    # get all the parameters for the command
    $cmd = Get-Command -Name $command
    $parameters = $cmd.Parameters.Values.GetEnumerator()

    # process all the variables that have the same prefix
    $variables = Get-Variable -Name "$prefix*"
    foreach ($parameter in $parameters) {
        $name = $parameter.Name

        # check if a variable with the same name exists
        $variable = $variables |? { $_.Name -eq "$prefix.$name" }
        if ($variable) {
            $newValue = $variable.Value
            $prevValue = $arguments[$name]

            # hashtables need special handling
            if ($newValue -is [Hashtable] -and $prevValue -is [Hashtable]) {
                # append each key-value-pair vs replacing the hashtable
                foreach ($kvp in $newValue.GetEnumerator()) {
                    $prevValue[$kvp.Key] = $kvp.Value
                }
            } else {
                # just assign the new value
                $arguments[$name] = $newValue
            }
        }

        # remove any arguments that cannot be null or empty
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
