# Development

Run commands from the repository root with PowerShell 7. Requirements: .NET SDK 10, a local RimWorld 1.7 installation and Harmony's 1.7 assembly. No game or dependency binaries are included in Git.

```powershell
./build.ps1
```

Override machine-specific paths when needed:

```powershell
./build.ps1 -RimWorldDir 'D:\SteamLibrary\steamapps\common\RimWorld' -HarmonyAssemblyPath 'D:\SteamLibrary\steamapps\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll'
```

The build rebuilds the mod, validates versions/XML/translations/preview/licensing and Harmony targets, runs the pure logic tests and installed-game IL contracts, then creates `artifacts/packages/WorkRework-<version>.zip`. Only the files in `tools/package-files.json` may enter the package; each ZIP entry is checked by SHA256.

`GroupedPawnsAssemblyPath` optionally points to the installed Grouped Pawns Lists assembly for additional metadata/IL contracts. If it is absent, those optional assembly contracts are skipped. The mod has no compile-time dependency on it.

Pure tests can run without the game:

```powershell
dotnet run --project Tests/WorkRework.LogicTests.csproj --configuration Release
```

They cover priority rules, model organization, repeated migrations and scrollbar visibility. They do not execute RimWorld, patch a running game, exercise Scribe or validate pixels and mouse interaction.

With RimWorld closed, `./deploy.ps1` backs up an existing local installation and replaces only the package allowlist. It does not delete unrelated files, change active mods, launch the game or upload to Steam. It requires write access to the configured Mods directory.

`./prepare-workshop.ps1` builds and creates a fresh clean upload folder and SHA256 manifest under `artifacts/workshop`. `-SkipBuild` accepts a previously built package only when all packaged sources still match. Neither mode uploads anything.

Keep the version synchronized between `Directory.Build.props`, `About/About.xml` and `workshop/listing.json`. The requested first release and Git tag are both `1.0.0`. Publishing a Steam item or a GitHub release is separate from creating a Git tag.

The existing MIT license is preserved. Do not commit game DLLs, decompiled source, private configuration, saves, logs or credentials. `artifacts`, intermediate outputs and local agent settings are ignored. `About/PublishedFileId.txt` contains the existing item identity **3826051737** and is included in packages to preserve it for subsequent updates. Never replace it with zero or create another item for an update.

Detailed historical checks and user feedback are in `TESTING.md`. Current project handoff is in `STATE.md`.
