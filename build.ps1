[CmdletBinding()]
param(
    [string]$RimWorldDir = 'D:\SteamLibrary\steamapps\common\RimWorld',
    [string]$HarmonyAssemblyPath = 'D:\SteamLibrary\steamapps\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll',
    [string]$GroupedPawnsAssemblyPath = 'D:\SteamLibrary\steamapps\workshop\content\294100\2340773428\1.6\Assemblies\PawnTableGrouped.0.dll'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$gameVersionPath = Join-Path $RimWorldDir 'Version.txt'
if (-not (Test-Path -LiteralPath $gameVersionPath)) { throw 'RimWorld Version.txt not found.' }
$gameVersion = (Get-Content -Raw -LiteralPath $gameVersionPath).Trim()
if ($gameVersion -notmatch '^1\.7\.') { throw "Expected RimWorld 1.7, found: $gameVersion" }
if (-not (Test-Path -LiteralPath $HarmonyAssemblyPath -PathType Leaf)) { throw 'Harmony 1.7 assembly not found.' }

$projectPath = Join-Path $projectRoot 'Source\WorkRework\WorkRework.csproj'
$buildDir = Join-Path $projectRoot 'artifacts\bin'
& dotnet build $projectPath --configuration Release --no-incremental --output $buildDir `
    --configfile (Join-Path $projectRoot 'NuGet.Config') `
    "-p:RimWorldDir=$RimWorldDir" "-p:HarmonyAssemblyPath=$HarmonyAssemblyPath"
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }

$modAssemblyDir = Join-Path $projectRoot '1.7\Assemblies'
New-Item -ItemType Directory -Path $modAssemblyDir -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $buildDir 'WorkRework.dll') -Destination (Join-Path $modAssemblyDir 'WorkRework.dll') -Force

& (Join-Path $projectRoot 'tools\check-project.ps1')
& (Join-Path $projectRoot 'tools\check-api.ps1') -RimWorldDir $RimWorldDir -HarmonyAssemblyPath $HarmonyAssemblyPath

$testDir = Join-Path $projectRoot 'artifacts\tests'
& dotnet build (Join-Path $projectRoot 'Tests\WorkRework.LogicTests.csproj') --configuration Release --output $testDir --configfile (Join-Path $projectRoot 'NuGet.Config')
if ($LASTEXITCODE -ne 0) { throw 'Priority logic test build failed.' }
$testArguments = @((Join-Path $RimWorldDir 'Assembly-CSharp.dll'))
if (Test-Path -LiteralPath $GroupedPawnsAssemblyPath -PathType Leaf) { $testArguments += $GroupedPawnsAssemblyPath }
else { Write-Output 'Grouped Pawns Lists not installed at the configured path; optional assembly contracts skipped.' }
& dotnet (Join-Path $testDir 'WorkRework.LogicTests.dll') @testArguments
if ($LASTEXITCODE -ne 0) { throw 'Priority logic or game IL contract checks failed.' }

$packageDir = Join-Path $projectRoot 'artifacts\packages'
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
[xml]$packageAbout = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'About\About.xml')
$packageVersion = [string]$packageAbout.ModMetaData.modVersion
$packagePath = Join-Path $packageDir "WorkRework-$packageVersion.zip"
$packageFiles = [ordered]@{}
foreach ($relativePath in (Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'tools/package-files.json') | ConvertFrom-Json)) {
    $packageFiles["WorkRework/$relativePath"] = $relativePath
}
$zipStream = [System.IO.File]::Open($packagePath, [System.IO.FileMode]::Create)
$zip = [System.IO.Compression.ZipArchive]::new($zipStream, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($entry in $packageFiles.GetEnumerator()) {
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $zip, (Join-Path $projectRoot $entry.Value), $entry.Key,
            [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally {
    $zip.Dispose()
    $zipStream.Dispose()
}

$archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    $expectedEntries = @($packageFiles.Keys | Sort-Object)
    $actualEntries = @($archive.Entries.FullName | Sort-Object)
    if (Compare-Object $expectedEntries $actualEntries) { throw 'Unexpected package contents.' }
    foreach ($entry in $archive.Entries) {
        $sourcePath = Join-Path $projectRoot $packageFiles[$entry.FullName]
        $sourceHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $sourcePath).Hash
        $entryStream = $entry.Open()
        $hasher = [System.Security.Cryptography.SHA256]::Create()
        try { $entryHash = [System.BitConverter]::ToString($hasher.ComputeHash($entryStream)).Replace('-', '') }
        finally { $hasher.Dispose(); $entryStream.Dispose() }
        if ($sourceHash -ne $entryHash) { throw "Package hash mismatch: $($entry.FullName)" }
    }
} finally { $archive.Dispose() }

Write-Output "Game reference: $gameVersion"
Write-Output "Package contents and SHA256 verified: $packagePath"
