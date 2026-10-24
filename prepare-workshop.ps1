[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [string]$RimWorldDir = 'D:\SteamLibrary\steamapps\common\RimWorld',
    [string]$HarmonyAssemblyPath = 'D:\SteamLibrary\steamapps\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
if (-not $SkipBuild) {
    & (Join-Path $projectRoot 'build.ps1') -RimWorldDir $RimWorldDir -HarmonyAssemblyPath $HarmonyAssemblyPath
}
[xml]$about = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'About/About.xml')
$version = [string]$about.ModMetaData.modVersion
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Invalid release version.' }
$relativeFiles = @(Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'tools/package-files.json') | ConvertFrom-Json)
$expectedEntries = @($relativeFiles | ForEach-Object { "WorkRework/$_" })
$zipPath = Join-Path $projectRoot "artifacts/packages/WorkRework-$version.zip"
$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    if (Compare-Object ($expectedEntries | Sort-Object) ($archive.Entries.FullName | Sort-Object)) { throw 'Unexpected package files.' }
    foreach ($entry in $archive.Entries) {
        $relative = $entry.FullName.Substring('WorkRework/'.Length)
        $stream = $entry.Open()
        $hasher = [Security.Cryptography.SHA256]::Create()
        try { $hash = [Convert]::ToHexString($hasher.ComputeHash($stream)) }
        finally { $hasher.Dispose(); $stream.Dispose() }
        if ($hash -ne (Get-FileHash -LiteralPath (Join-Path $projectRoot $relative) -Algorithm SHA256).Hash) {
            throw "Stale package: $relative changed since the last build."
        }
    }
} finally { $archive.Dispose() }

# Create a new staging folder each time. Never upload the development checkout.
$stamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fffffff')
$stageRoot = Join-Path $projectRoot "artifacts/workshop/WorkRework-$version-$stamp"
New-Item -ItemType Directory -Path $stageRoot | Out-Null
Expand-Archive -LiteralPath $zipPath -DestinationPath $stageRoot
$content = Join-Path $stageRoot 'WorkRework'
$actualFiles = @(Get-ChildItem -LiteralPath $content -Recurse -File | ForEach-Object { [IO.Path]::GetRelativePath($content, $_.FullName).Replace('\', '/') })
if (Compare-Object ($relativeFiles | Sort-Object) ($actualFiles | Sort-Object)) { throw 'Staging folder contents differ from package.' }
$files = @()
foreach ($relative in $relativeFiles) {
    $path = Join-Path $content $relative
    $sourceHash = (Get-FileHash -LiteralPath (Join-Path $projectRoot $relative) -Algorithm SHA256).Hash
    $stagedHash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    if ($sourceHash -ne $stagedHash) { throw "Staging hash mismatch: $relative" }
    $files += [ordered]@{ path = $relative; bytes = (Get-Item -LiteralPath $path).Length; sha256 = $stagedHash }
}
$manifest = [ordered]@{
    version = $version
    packageId = [string]$about.ModMetaData.packageId
    appId = 294100
    preparedUtc = [DateTime]::UtcNow.ToString('o')
    repository = 'https://github.com/Diablood/work-rework'
    files = $files
    steamUploadPerformed = $false
}
$manifestPath = Join-Path $stageRoot 'manifest.json'
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $manifestPath -Encoding utf8NoBOM
Write-Output "Workshop content prepared: $content"
Write-Output "Manifest: $manifestPath"
Write-Output "All $($files.Count) staged file hashes verified. No Steam upload performed."
