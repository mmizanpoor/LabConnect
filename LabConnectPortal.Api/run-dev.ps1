param(
    [string]$LaunchProfile = 'http'
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
& "$scriptDir\scripts\free-port.ps1"
Set-Location $scriptDir
dotnet run --launch-profile $LaunchProfile @args
