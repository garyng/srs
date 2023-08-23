[CmdletBinding()]
param (
    [Parameter()][switch]$test
)

$cliArgs = @(
    "compose",
    "-f", "$PSScriptRoot/../src/docker-compose.yml"
)

if (!$test) {
    $cliArgs += @("up")
} else {
    $cliArgs += @(
        "-f", "$PSScriptRoot/../src/docker-compose.test.yml",
        "up",
        "srs.test.db"
    )
}

& docker @cliArgs