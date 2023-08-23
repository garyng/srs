[CmdletBinding()]
param (
    [Parameter()][switch]$test
)

$cliArgs = @(
    "compose",
    "-f", "$PSScriptRoot/../src/docker-compose.yml"
)

if ($test) {
    $cliArgs += @(
        "-f", "$PSScriptRoot/../src/docker-compose.test.yml"
    )
}

$cliArgs += @(
    "down",
    "-v"
)

& docker @cliArgs