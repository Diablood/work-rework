[CmdletBinding()]
param(
    [string]$RimWorldDir = 'D:\SteamLibrary\steamapps\common\RimWorld',
    [string]$HarmonyAssemblyPath = 'D:\SteamLibrary\steamapps\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$sdkVersion = (& dotnet --version).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Cannot determine the current .NET SDK.' }
$dotnetRoot = Split-Path -Parent (Get-Command dotnet -CommandType Application).Source
Add-Type -Path (Join-Path $dotnetRoot "sdk\$sdkVersion\System.Reflection.MetadataLoadContext.dll")
$modDll = Join-Path $projectRoot '1.7\Assemblies\WorkRework.dll'
[string[]]$paths = @((Get-ChildItem -LiteralPath $RimWorldDir -Filter '*.dll').FullName) + @($HarmonyAssemblyPath, $modDll)
$context = [System.Reflection.MetadataLoadContext]::new([System.Reflection.PathAssemblyResolver]::new($paths), 'System.Private.CoreLib')
try {
    $game = $context.LoadFromAssemblyPath((Join-Path $RimWorldDir 'Assembly-CSharp.dll'))
    $mod = $context.LoadFromAssemblyPath($modDll)
    if (@($mod.GetReferencedAssemblies() | Where-Object { $_.Name -like '*PawnTableGrouped*' -or $_.Name -like '*RWLayout*' }).Count -gt 0) {
        throw 'Optional group compatibility must not introduce assembly dependencies.'
    }
    $flags = [System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
    $patchTypes = @($mod.GetTypes() | Where-Object { $_.GetCustomAttributesData().AttributeType.FullName -contains 'HarmonyLib.HarmonyPatch' })
    if ($patchTypes.Count -ne 24) { throw "Expected 24 patch classes, found $($patchTypes.Count)." }
    foreach ($patchType in $patchTypes) {
        $patchAttribute = $patchType.GetCustomAttributesData() | Where-Object { $_.AttributeType.FullName -eq 'HarmonyLib.HarmonyPatch' }
        $targetTypeName = $patchAttribute.ConstructorArguments[0].Value.FullName
        $targetName = [string]$patchAttribute.ConstructorArguments[1].Value
        $targetType = $game.GetType($targetTypeName, $true)
        if ($patchAttribute.ConstructorArguments.Count -eq 3) { $targetName = 'get_' + $targetName }
        $target = @($targetType.GetMethods($flags) | Where-Object Name -eq $targetName)
        if ($target.Count -ne 1) { throw "Expected exactly one target: $targetTypeName.$targetName" }
        foreach ($patchMethod in $patchType.GetMethods($flags)) {
            if ($patchMethod.Name -eq 'Transpiler') { continue }
            foreach ($parameter in $patchMethod.GetParameters()) {
                $name = $parameter.Name
                if ($name.StartsWith('___')) {
                    $field = $targetType.GetField($name.Substring(3), $flags)
                    if ($null -eq $field) { throw "Missing patched field: $targetTypeName.$name" }
                    $expected = $field.FieldType
                } elseif ($name -eq '__instance') { $expected = $targetType
                } elseif ($name -eq '__result') { $expected = $target[0].ReturnType
                } elseif ($name -eq '__state') { continue
                } else {
                    $original = @($target[0].GetParameters() | Where-Object Name -eq $name)
                    if ($original.Count -ne 1) { throw "Missing original parameter: $targetName.$name" }
                    $expected = $original[0].ParameterType
                }
                $actual = $parameter.ParameterType
                if ($actual.IsByRef) { $actual = $actual.GetElementType() }
                if ($actual.FullName -ne $expected.FullName) { throw "Patch parameter mismatch: $($patchType.FullName).$name" }
            }
        }
        Write-Output "Patch signature verified: $targetTypeName.$targetName"
    }
    $windowType = $game.GetType('RimWorld.MainTabWindow_PawnTable', $true)
    $workerType = $game.GetType('RimWorld.PawnColumnWorker_WorkPriority', $true)
    $labelRect = $workerType.GetMethod('GetLabelRect', $flags)
    if ($null -eq $labelRect -or $labelRect.ReturnType.FullName -ne 'UnityEngine.Rect' -or
        $labelRect.GetParameters().Count -ne 1 -or $labelRect.GetParameters()[0].ParameterType.FullName -ne 'UnityEngine.Rect') {
        throw 'Native header label hit-test signature changed.'
    }
    $headerClick = $workerType.GetMethod('HeaderClicked', $flags)
    if ($null -eq $headerClick -or $headerClick.ReturnType.FullName -ne 'System.Void' -or
        $headerClick.GetParameters().Count -ne 2 -or $headerClick.GetParameters()[0].ParameterType.FullName -ne 'UnityEngine.Rect' -or
        $headerClick.GetParameters()[1].ParameterType.FullName -ne 'RimWorld.PawnTable') {
        throw 'Native header bulk/sort action signature changed.'
    }
    if ($game.GetType('Verse.Pawn', $true).GetProperty('CurJob').PropertyType.FullName -ne 'Verse.AI.Job' -or
        $game.GetType('Verse.AI.Job', $true).GetField('workGiverDef').FieldType.FullName -ne 'RimWorld.WorkGiverDef') {
        throw 'Current job/work-giver contract changed.'
    }
    if ($game.GetType('RimWorld.Pawn_WorkSettings', $true).GetProperty('Initialized').PropertyType.FullName -ne 'System.Boolean' -or
        $mod.GetType('WorkRework.WorkReworkMod', $true).GetProperty('MaximumPriority', $flags).PropertyType.FullName -ne 'System.Int32') {
        throw 'Configured range/initialization contract changed.'
    }
    $background = $game.GetType('RimWorld.WidgetsWork', $true).GetMethod('DrawWorkBoxBackground', $flags)
    if ($null -eq $background -or -not $background.IsStatic -or $background.ReturnType.FullName -ne 'System.Void' -or
        ($background.GetParameters().ParameterType.FullName -join ',') -ne 'UnityEngine.Rect,Verse.Pawn,Verse.WorkTypeDef') {
        throw 'Native work-box background signature changed.'
    }
    if ($mod.GetType('WorkRework.WorkReworkSettings', $true).BaseType.FullName -ne 'Verse.ModSettings') {
        throw 'Unexpected global settings type.'
    }
    $component = $mod.GetType('WorkRework.WorkPriorityComponent', $true)
    if ($component.BaseType.FullName -ne 'Verse.GameComponent' -or
        $component.GetConstructors().GetParameters().ParameterType.FullName -ne 'Verse.Game') {
        throw 'Unexpected persistent game-component contract.'
    }
    if ($windowType.GetField('table', $flags).FieldType.FullName -ne 'RimWorld.PawnTable') { throw 'Window table field changed.' }
    if ($mod.GetType('WorkRework.PawnColumnWorker_WorkGiverPreview', $true).BaseType.FullName -ne 'RimWorld.PawnColumnWorker') { throw 'Unexpected preview worker type.' }
} finally { $context.Dispose() }
Write-Output 'API checks passed without executing game code. Runtime patching still requires an in-game test.'
