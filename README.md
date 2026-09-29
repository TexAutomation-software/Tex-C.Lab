# TEX C LAB

## Building/testing

```
dotnet build
dotnet run
```

## Generazione setup

1. run the following to generate a "packaged" executable
```
dotnet publish -c Release -r win-x64 --self-contained true
```
2. Run the Setup/Setup-script.iss script
3. Installer is in Setup/Output