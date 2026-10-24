[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$iconPath = Join-Path $projectRoot 'About\ModIcon.png'
$iconBytes = [System.IO.File]::ReadAllBytes($iconPath)
if ($iconBytes.Length -lt 24 -or [System.BitConverter]::ToString($iconBytes, 0, 8) -ne '89-50-4E-47-0D-0A-1A-0A') {
    throw 'About/ModIcon.png is not a PNG file.'
}
$previewPath = Join-Path $projectRoot 'About\Preview.png'
$preview = [System.IO.File]::ReadAllBytes($previewPath)
if ($preview.Length -lt 24 -or $preview.Length -ge 1000000 -or [System.BitConverter]::ToString($preview, 0, 8) -ne '89-50-4E-47-0D-0A-1A-0A') {
    throw 'Workshop preview must be a PNG smaller than 1,000,000 bytes.'
}
$previewWidth = [System.Net.IPAddress]::NetworkToHostOrder([System.BitConverter]::ToInt32($preview, 16))
$previewHeight = [System.Net.IPAddress]::NetworkToHostOrder([System.BitConverter]::ToInt32($preview, 20))
if ($previewWidth -lt 256 -or $previewWidth -ne $previewHeight) { throw 'Workshop preview must be square and at least 256px.' }
if (-not (Test-Path -LiteralPath (Join-Path $projectRoot 'LICENSE') -PathType Leaf) -or
    -not (Test-Path -LiteralPath (Join-Path $projectRoot 'LICENSING.md') -PathType Leaf)) { throw 'Release licensing files are missing.' }
[xml]$english = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'Languages\English\Keyed\WorkRework.xml')
[xml]$french = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'Languages\French\Keyed\WorkRework.xml')
$englishKeys = @($english.LanguageData.ChildNodes | Where-Object NodeType -eq 'Element' | ForEach-Object Name)
$frenchKeys = @($french.LanguageData.ChildNodes | Where-Object NodeType -eq 'Element' | ForEach-Object Name)
if ($englishKeys.Count -ne @($englishKeys | Select-Object -Unique).Count -or
    $frenchKeys.Count -ne @($frenchKeys | Select-Object -Unique).Count -or
    (Compare-Object ($englishKeys | Sort-Object) ($frenchKeys | Sort-Object))) {
    throw 'Translation keys are duplicated or do not match between French and English.'
}
foreach ($key in $englishKeys) {
    $enArgs = @([regex]::Matches($english.LanguageData.$key, '\{\d+\}') | ForEach-Object Value | Sort-Object -Unique)
    $frArgs = @([regex]::Matches($french.LanguageData.$key, '\{\d+\}') | ForEach-Object Value | Sort-Object -Unique)
    if (Compare-Object $enArgs $frArgs) { throw "Translation placeholders do not match: $key" }
}
[xml]$about = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'About\About.xml')
[xml]$versionProps = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'Directory.Build.props')
$expectedVersion = [string]$versionProps.Project.PropertyGroup.Version
if ($expectedVersion -notmatch '^\d+\.\d+\.\d+$' -or $about.ModMetaData.modVersion -cne $expectedVersion) {
    throw 'About.xml modVersion must match the numeric version in Directory.Build.props.'
}
$listing = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'workshop/listing.json') | ConvertFrom-Json
if ($listing.version -cne $expectedVersion -or $listing.gitTag -cne $expectedVersion -or
    $listing.packageId -cne $about.ModMetaData.packageId -or $listing.title -cne $about.ModMetaData.name) {
    throw 'Workshop listing identity, version and Git tag must match the release metadata.'
}
$publishedFileId = (Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'About/PublishedFileId.txt')).Trim()
if ($publishedFileId -notmatch '^[1-9][0-9]+$' -or $publishedFileId -cne [string]$listing.publishedFileId) {
    throw 'PublishedFileId.txt must match the existing Workshop item in listing.json.'
}
[xml]$folders = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'LoadFolders.xml')
if ($about.ModMetaData.packageId -cne 'diablood.workrework') { throw 'Unexpected packageId.' }
if (@($about.ModMetaData.supportedVersions.li).Count -ne 1 -or $about.ModMetaData.supportedVersions.li -ne '1.7') {
    throw 'This baseline targets only RimWorld 1.7.'
}
if ($about.ModMetaData.modDependencies.li.packageId -ne 'brrainz.harmony') { throw 'Harmony dependency is missing.' }
if (@($about.ModMetaData.loadAfter.li) -notcontains 'brrainz.harmony') { throw 'Harmony load order is missing.' }
if (@($folders.loadFolders.'v1.7'.li) -notcontains '1.7') { throw 'Versioned assemblies folder is not loaded.' }
$assemblyDir = Join-Path $projectRoot '1.7\Assemblies'
$assemblyPath = Join-Path $assemblyDir 'WorkRework.dll'
if (-not (Test-Path -LiteralPath $assemblyPath -PathType Leaf)) { throw 'Build WorkRework.dll first.' }
$unexpectedFiles = @(Get-ChildItem -LiteralPath $assemblyDir -File | Where-Object { $_.Name -ne 'WorkRework.dll' })
if ($unexpectedFiles.Count -ne 0) { throw 'Unexpected file in mod assemblies; do not redistribute game or dependency assemblies.' }
$assemblyIdentity = [System.Reflection.AssemblyName]::GetAssemblyName($assemblyPath)
if ($assemblyIdentity.Name -ne 'WorkRework') { throw 'Unexpected assembly identity.' }
if ($assemblyIdentity.Version.ToString(3) -ne $expectedVersion -or $assemblyIdentity.Version.Revision -ne 0) {
    throw 'Built assembly version does not match About.xml and Directory.Build.props.'
}
$fileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($assemblyPath).FileVersion
if ($fileVersion -ne "$expectedVersion.0") { throw 'DLL file version does not match modVersion.' }
Write-Output "Project checks passed: mod/DLL version $expectedVersion, XML, icon/preview PNG, licensing, French/English keys and placeholders, package identity, dependencies and assembly contents."
