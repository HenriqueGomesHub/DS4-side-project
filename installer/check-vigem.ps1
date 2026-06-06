# Reports whether the ViGEmBus kernel driver is installed and registered.
# Exit codes: 0 = installed, 1 = missing.

$drivers = Get-WmiObject Win32_PnPSignedDriver -ErrorAction SilentlyContinue |
    Where-Object { $_.DeviceName -match 'ViGEm|Nefarius' }

if ($drivers) {
    Write-Host "ViGEmBus detected:"
    $drivers | Select-Object DeviceName, DriverVersion | Format-Table -AutoSize
    exit 0
}

Write-Host "ViGEmBus driver NOT detected."
Write-Host "Install from: https://github.com/nefarius/ViGEmBus/releases"
exit 1
