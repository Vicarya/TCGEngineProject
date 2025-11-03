# Copy native SQLite dlls from NuGet cache to Unity Plugins
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\copy_nuget_to_plugins.ps1

$packageId = "sqlitepclraw.bundle_e_sqlite3"
$version = "3.0.2"
$pkgRoot = Join-Path $env:USERPROFILE ".nuget\packages\$packageId\$version"
Write-Host "PackageRoot:" $pkgRoot
if (-not (Test-Path $pkgRoot)) {
    Write-Error "Package not found: $pkgRoot"
    exit 1
}

# Try the standard location first
$nativeDir = Join-Path $pkgRoot "runtimes\win-x64\native"
$found = $false
if (Test-Path $nativeDir) {
    Write-Host "Found native dir:" $nativeDir
    $found = $true
    $dlls = Get-ChildItem -Path $nativeDir -Filter *.dll -File -ErrorAction SilentlyContinue
}

# If not found, search the package for likely dlls
if (-not $found) {
    Write-Host "Standard native dir not found, doing recursive search for sqlite native DLLs..."
    $dlls = Get-ChildItem -Path $pkgRoot -Recurse -File -Include *.dll | Where-Object { $_.Name -match "e_sqlite3|sqlite3|SQLite.Interop" }
    if ($dlls -and $dlls.Count -gt 0) { $found = $true }
}

if (-not $found -or -not $dlls) {
    Write-Error "No native dlls found in package."
    exit 1
}

$dst = Join-Path (Resolve-Path .).Path "Assets\Plugins\x86_64"
if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst | Out-Null }
foreach ($f in $dlls) {
    Write-Host "Copying $($f.Name) -> $dst"
    Copy-Item -Path $f.FullName -Destination $dst -Force
}

Write-Host "Destination files:" 
Get-ChildItem -Path $dst -File | Select-Object Name,FullName | Format-Table -AutoSize
Write-Host "Done. Please restart Unity or call AssetDatabase.Refresh() in Editor to recompile."