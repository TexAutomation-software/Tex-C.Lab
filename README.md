# TEX C LAB

## Requirements

The following software must be installed on the build machine:

- .NET 9 SDK
- Inno Setup 6
- MiKTeX
- PowerShell 7 (recommended)

Verify installation:

```powershell
dotnet --version
pdflatex --version
ISCC.exe
```

---

## Building / Testing

Build:

```powershell
dotnet build
```

Run:

```powershell
dotnet run
```

---

## Version Management

The only source of truth for application versioning is the project file:

```xml
<Version>0.0.1</Version>
<BuildDate>20261005</BuildDate>
```

Before generating a release, update:

- `Version`
- `BuildDate`

The release process automatically propagates these values to:

- Application metadata
- Installer version
- User Manual
- Changelog

---

## Release Procedure

Preferred method:

```powershell
.\Release.ps1
```

Release workflow:

1. Update source code
2. Update changelog contents
3. Update `<Version>`
4. Update `<BuildDate>`
5. Run `Release.ps1`

The script automatically:

- Updates documentation metadata
- Rebuilds `UserManual.pdf`
- Rebuilds `Changelog.pdf`
- Publishes the application
- Generates the installer

Generated artifacts:

```text
Docs/build/UserManual.pdf
Changelog/build/Changelog.pdf
Setup/Output/
```

To automatically launch the generated installer:

```powershell
.\Release.ps1 -LaunchInstaller
```

---

## Manual Installer Generation

Prefer using `Release.ps1`.

Manual procedure:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Then compile:

```text
Setup/Setup-script.iss
```

Generated installer:

```text
Setup/Output/
```

---

## Online Update System

The update logic is mainly implemented in:

```text
Services/