param(
    [Parameter(Mandatory = $false)]
    [int[]]$Ports = @(5299, 7182)
)

$allowedProcessNames = @('dotnet', 'LabConnectPortal.Api', 'iisexpress')

function Get-ListenerPids([int]$Port) {
    $pids = @()
    $pattern = ":$Port\s+.*LISTENING"
    netstat -ano | Select-String $pattern | ForEach-Object {
        $parts = ($_.Line -replace '\s+', ' ').Trim() -split ' '
        $pidValue = $parts[-1]
        if ($pidValue -match '^\d+$') {
            $pids += [int]$pidValue
        }
    }
    return $pids | Select-Object -Unique
}

foreach ($port in $Ports) {
    $pids = Get-ListenerPids -Port $port
    if (-not $pids) {
        continue
    }

    foreach ($procId in $pids) {
        $proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
        if (-not $proc) {
            continue
        }

        if ($allowedProcessNames -contains $proc.ProcessName) {
            Write-Host "Freeing port ${port}: stopping $($proc.ProcessName) (PID $procId)"
            Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
        } else {
            Write-Warning "Port $port is used by $($proc.ProcessName) (PID $procId). Stop it manually or change the port."
        }
    }
}

Start-Sleep -Milliseconds 400
