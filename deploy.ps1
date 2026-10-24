[CmdletBinding()]
param(
    [string]$RimWorldModsDir = 'D:\SteamLibrary\steamapps\common\RimWorld\Mods'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
[xml]$sourceAbout = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'About\About.xml')
$packageVersion = [string]$sourceAbout.ModMetaData.modVersion
if ($packageVersion -notmatch '^\d+\.\d+\.\d+$') { throw 'Invalid numeric mod version.' }
$packagePath = Join-Path $projectRoot "artifacts\packages\WorkRework-$packageVersion.zip"
$targetDir = Join-Path $RimWorldModsDir 'WorkRework'
if (-not (Test-Path -LiteralPath $RimWorldModsDir -PathType Container)) { throw 'RimWorld Mods directory not found.' }
if (Get-Process -Name 'RimWorld*' -ErrorAction SilentlyContinue) { throw 'Close RimWorld before deploying the test package.' }
$expectedEntries = @(Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'tools/package-files.json') | ConvertFrom-Json | ForEach-Object { "WorkRework/$_" })
$archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    if (Compare-Object ($expectedEntries | Sort-Object) ($archive.Entries.FullName | Sort-Object)) {
        throw 'Unexpected package contents; deployment stopped.'
    }
} finally { $archive.Dispose() }

if (Test-Path -LiteralPath $targetDir) {
    if ((Get-Item -LiteralPath $targetDir).Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw 'Deployment target must not be a junction or symbolic link.'
    }
    [xml]$existingAbout = Get-Content -Raw -LiteralPath (Join-Path $targetDir 'About\About.xml')
    if ($existingAbout.ModMetaData.packageId -ne 'diablood.workrework') { throw 'Existing folder belongs to another mod.' }
    $installedDll = Join-Path $targetDir '1.7\Assemblies\WorkRework.dll'
    if (Test-Path -LiteralPath $installedDll) {
        $lockCheck = [System.IO.File]::Open($installedDll, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
        $lockCheck.Dispose()
    }
    $backupDir = Join-Path $projectRoot 'artifacts\backups'
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    $backupPath = Join-Path $backupDir ('WorkRework-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fffffff') + '.zip')
    Compress-Archive -LiteralPath $targetDir -DestinationPath $backupPath
    Write-Output "Existing installation backed up: $backupPath"
}

# No deletion: replace only the explicitly allowed package files.
Expand-Archive -LiteralPath $packagePath -DestinationPath $RimWorldModsDir -Force
$archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    foreach ($entry in $archive.Entries) {
        $entryStream = $entry.Open()
        $hasher = [System.Security.Cryptography.SHA256]::Create()
        try { $packageHash = [System.BitConverter]::ToString($hasher.ComputeHash($entryStream)).Replace('-', '') }
        finally { $hasher.Dispose(); $entryStream.Dispose() }
        $installedPath = Join-Path $RimWorldModsDir $entry.FullName
        if ((Get-FileHash -Algorithm SHA256 -LiteralPath $installedPath).Hash -ne $packageHash) {
            throw "Installed file hash mismatch: $($entry.FullName)"
        }
    }
} finally { $archive.Dispose() }
Write-Output "Work Rework $packageVersion installed; all $($expectedEntries.Count) file hashes verified: $targetDir"
