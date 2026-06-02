# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Coding-Vorgaben

**Sprache & UI-Texte**
- Alle sichtbaren Texte in der App (Labels, Buttons, Statusmeldungen, Fehlertexte) sind auf Deutsch
- Bezeichner (Klassen, Methoden, Variablen) bleiben auf Englisch

**Kommentare**
- Kein Kommentar im Code außer wenn das *Warum* nicht aus dem Code selbst ersichtlich ist
- Keine XML-Docstrings, keine mehrzeiligen Kommentarblöcke

**Async**
- `async void` ausschließlich für Event-Handler (z.B. `_Click`, `_Load`, `_Activated`)
- Alle anderen asynchronen Methoden geben `Task` oder `Task<T>` zurück
- Niemals `.Result` oder `.GetAwaiter().GetResult()` auf Tasks aufrufen

**Git & Releases**
- Vor jedem Commit `dotnet build` ausführen und sicherstellen dass 0 Fehler vorliegen
- Releases ausschließlich über Git-Tags auslösen (`git tag vX.Y.Z && git push origin vX.Y.Z`)
- `codesign.pfx` und alle `.pfx`-Dateien niemals einchecken
- `min-version.json` nur dann ändern, wenn ein Pflichtupdate für ältere Versionen gewünscht ist

## Build & Entwicklung

```powershell
# Debug-Build
dotnet build VelopackDemo/VelopackDemo.csproj

# Release-Publish (self-contained, wie CI)
dotnet publish VelopackDemo/VelopackDemo.csproj -c Release -r win-x64 --self-contained true -o VelopackDemo/publish /p:Version=1.0.0

# Lokales Velopack-Paket erstellen (vpk muss installiert sein)
dotnet tool install -g vpk
vpk pack --packId VelopackDemo --packVersion 1.0.0 --packDir VelopackDemo/publish --mainExe VelopackDemo.exe --icon VelopackDemo/app.ico --outputDir VelopackDemo/releases
```

## Release veröffentlichen

Releases werden ausschließlich über Git-Tags ausgelöst – nie manuell:

```powershell
git tag v1.2.3
git push origin v1.2.3
```

Der GitHub Actions Workflow (`.github/workflows/release.yml`) baut dann automatisch, signiert und erstellt das GitHub Release. Die Version der App wird aus dem Tag abgeleitet (`/p:Version=` beim Publish).

## Architektur

**`Program.cs`** – Einstiegspunkt. `VelopackApp.Build().Run()` muss zwingend als allererstes aufgerufen werden (vor jeder anderen Logik), damit Velopack Install/Uninstall-Hooks verarbeiten kann.

**`Form1.cs`** – Enthält die gesamte Update-Logik:
- `Form1_Load` → prüft ob ein Pflichtupdate erforderlich ist (`IsForcedUpdateRequiredAsync`)
- `Form1_Activated` → prüft einmal pro Tag still nach Updates
- `_isUpdating`-Flag verhindert parallele `UpdateManager`-Instanzen (würde zu Datei-Lock-Konflikten führen)
- `CurrentVersion` parst `Application.ProductVersion` mit `.Split('+')[0]` – notwendig weil .NET den Git-Commit-Hash als `1.0.0+<hash>` anhängt

**`min-version.json`** (Repo-Root) – Steuert Pflichtupdate-Logik. Wird live von GitHub raw gelesen. Wenn die installierte Version kleiner als `minimumRequiredVersion` ist, wird das Update erzwungen ohne Nutzerinteraktion:

```json
{ "minimumRequiredVersion": "1.0.5" }
```

## GitHub Secrets

Für den Signing-Schritt im Workflow werden zwei Secrets benötigt:

| Secret | Inhalt |
|---|---|
| `CERT_PFX` | Base64-kodierter Inhalt von `codesign.pfx` |
| `CERT_PASSWORD` | Passwort für das PFX-Zertifikat |

`codesign.pfx` liegt lokal unter `VelopackDemo/codesign.pfx` und ist nicht im Repository eingecheckt (`.gitignore`).

## .gitignore

`bin/`, `obj/`, `publish/`, `releases/` und `*.user` sind ignoriert. `codesign.pfx` darf nie eingecheckt werden.
