# Download latest sqlite win64 DLL from sqlite.org and copy sqlite3.dll to Assets/Plugins/x86_64
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\download_sqlite3_and_copy.ps1

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path .).Path
$dstDir = Join-Path $projectRoot "Assets\Plugins\x86_64"
if (-not (Test-Path $dstDir)) { New-Item -ItemType Directory -Path $dstDir | Out-Null }

Write-Host "Fetching sqlite download page..."
$downloadPage = Invoke-WebRequest -Uri "https://www.sqlite.org/download.html" -UseBasicParsing
$content = $downloadPage.Content

# Try to find absolute URL first
$absPattern = 'href\s*=\s*"(https?://[^"]*sqlite-dll-win64-x64[^"]*\.zip)"'
$relPattern = 'href\s*=\s*"(/[^\"]*sqlite-dll-win64-x64[^\"]*\.zip)"'
# Try to find absolute URL first (match sqlite.org naming: sqlite-dll-win-x64-*.zip)
$absPattern = 'href\s*=\s*"(https?://[^\"]*sqlite-dll-win-x64[^\"]*\\.zip)"'
$relPattern = 'href\s*=\s*"(/[^\"]*sqlite-dll-win-x64[^\"]*\\.zip)"'
$zipUrl = $null

$m = [regex]::Match($content, $absPattern)
if ($m.Success) { $zipUrl = $m.Groups[1].Value }
else {
    $m2 = [regex]::Match($content, $relPattern)
    if ($m2.Success) { $zipUrl = "https://www.sqlite.org" + $m2.Groups[1].Value }
}

# Fallback: sqlite.org uses a JS mapping like d391('a12','2025/sqlite-dll-win-x64-3500400.zip');
if (-not $zipUrl) {
    $mapPattern = "d391\('\w+','([^']*sqlite-dll-win-x64[^']*)'\)"
    $m3 = [regex]::Match($content, $mapPattern)
    if ($m3.Success) {
        $rel = $m3.Groups[1].Value
        # make sure it is an absolute URL
        if ($rel -match '^https?://') { $zipUrl = $rel } else { $zipUrl = "https://www.sqlite.org/" + $rel }
    }
}

if (-not $zipUrl) {
    Write-Error "Could not find sqlite win64 zip URL on page. (checked hrefs and JS mapping)"
    exit 1
}

Write-Host "Found zip URL: $zipUrl"
$tmp = Join-Path $env:TEMP "sqlite_dl_$(Get-Random)"
if (Test-Path $tmp) { Remove-Item -Recurse -Force $tmp }
New-Item -ItemType Directory -Path $tmp | Out-Null
$zipFile = Join-Path $tmp "sqlite.zip"
Write-Host "Downloading zip to $zipFile..."
Invoke-WebRequest -Uri $zipUrl -OutFile $zipFile -UseBasicParsing

Write-Host "Expanding archive..."
Expand-Archive -Path $zipFile -DestinationPath $tmp -Force

# find sqlite3.dll
$found = Get-ChildItem -Path $tmp -Recurse -File -Filter "sqlite3.dll" -ErrorAction SilentlyContinue
if (-not $found -or $found.Count -eq 0) {
    Write-Error "sqlite3.dll not found inside archive. Listing top-level files for debugging:"
    Get-ChildItem -Path $tmp -Recurse -File | Select-Object -First 50 | ForEach-Object { Write-Host $_.FullName }
    exit 1
}

$sourceDll = $found[0].FullName
$dstDll = Join-Path $dstDir "sqlite3.dll"
Copy-Item -Path $sourceDll -Destination $dstDll -Force
Write-Host "Copied sqlite3.dll -> $dstDll"

# Also copy as e_sqlite3.dll for compatibility
$dstDll2 = Join-Path $dstDir "e_sqlite3.dll"
Copy-Item -Path $sourceDll -Destination $dstDll2 -Force
Write-Host "Also copied as e_sqlite3.dll -> $dstDll2"

Write-Host "Done. Please restart Unity or run AssetDatabase.Refresh() in Editor to recompile."