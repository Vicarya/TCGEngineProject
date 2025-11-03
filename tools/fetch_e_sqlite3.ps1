# Fetch native SQLite bundle (e_sqlite3) from NuGet and copy native DLLs to Assets/Plugins
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\fetch_e_sqlite3.ps1

$OutPlugins = "Assets/Plugins"
$PackageId = "SQLitePCLRaw.bundle_e_sqlite3"
$tmp = Join-Path $env:TEMP ("nuget_" + $PackageId)
if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue }
New-Item -ItemType Directory -Path $tmp | Out-Null

$pkgUrl = "https://www.nuget.org/api/v2/package/$PackageId"
$pkgFile = Join-Path $tmp ($PackageId + ".nupkg")
Write-Host "Downloading $PackageId ..."
Invoke-WebRequest -Uri $pkgUrl -OutFile $pkgFile -UseBasicParsing

Write-Host "Expanding package..."
# Some PowerShell versions disallow Extracting .nupkg directly; treat it as zip
$zipFile = $pkgFile + ".zip"
Copy-Item -Path $pkgFile -Destination $zipFile -Force
try {
    Expand-Archive -Path $zipFile -DestinationPath $tmp -Force
} catch {
    Write-Error "Failed to expand package: $_"
    Exit 1
}

# Look for runtimes/*/native
$runtimes = Join-Path $tmp "runtimes"
if (-not (Test-Path $runtimes)) { Write-Warning "No runtimes folder found in package."; exit 1 }

# Copy native DLLs to Assets/Plugins/<rid> (e.g. win-x64 -> x86_64)
Get-ChildItem -Path $runtimes -Directory | ForEach-Object {
    $rid = $_.Name
    $native = Join-Path $_.FullName "native"
    if (-not (Test-Path $native)) { return }
    # map rid to target folder name
    $target = switch -Wildcard ($rid) {
        "*win-x64*" { Join-Path $OutPlugins "x86_64" }
        "*win-x86*" { Join-Path $OutPlugins "x86" }
        default { Join-Path $OutPlugins $rid }
    }
    if (-not (Test-Path $target)) { New-Item -ItemType Directory -Path $target | Out-Null }
    Get-ChildItem -Path $native -File -Filter *.dll | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination (Join-Path $target $_.Name) -Force
        Write-Host "Copied $($_.Name) -> $target"
    }
}

Write-Host "Done. Restart Unity or call AssetDatabase.Refresh() in Editor."