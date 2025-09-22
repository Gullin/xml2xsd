# readme
Källkod för enkel console-applikation som tar olika typer av sökvägar till xml-filer.
xml-filer utgör underlag för att generera ett xsd-schema. Används till fördel för
att validera xml-dokument. Desto fler xml-filer som utgör 


## Förutsättning
Installerad runtime för .Net om endast köra (se version i [csproj-fil](src/xml2xsd.csproj) under TargetFramework).
Ska projektet byggas behövs hela .Net SDK installeras.

[.Net-download](https://dot.net/download)


## Skapar nytt console-projekt i relativ sökväg
körs endast en gång och är redan gjord

```bash
dotnet new console -n xml2xsd -o .\src
```


## Publish/Deploy (kräver .Net SDK)
Standardparametrar
```bash
dotnet publish -c Release -o ./publish 
```

Standalone. Packar med hela och nödvändiga delar av .Net. Tar lite längre tid men optimering sker mot plattform och för vilka beroenden som packas med. Fördel att den kan köras på plattform oberoende av något annat. Nackdel att applikationer blir stora (kanske onödigt stora).
```bash
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:Trim=true --self-contained true -o ./publish 
```
Runtimeidentifiers som win-x64 går att se [här](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog).
### Parametrar
Ingen .vscode med ev. launch.json eller tasks.json används.
Ändra förutsättningar i csproj-filen under relevant PropertyGroup.
```xml
  <!-- Produktnamn -->
  <AssemblyName>xml2xsd</AssemblyName>
  <!-- Filbeskrivning -->
  <AssemblyTitle></AssemblyTitle>
  <!-- Filversion -->
  <FileVersion></FileVersion>
  <!-- Produktversion -->
  <InformationalVersion></InformationalVersion>
  <!-- Copyright -->
  <Copyright></Copyright>

```


## Bygg för debug ifrån rotkatalog
```bash
dotnet run --project .\src
```


## Körning av program
### Utan kompilering eller Publish/Deploy
Körs med .Net SDK installerat och i project/solution-katalog.
```bash
dotnet run "{sökväg1}" {sökväg2}
```
### Förkompilerad/publicerad
```bash
xml2xsd.exe "{sökväg1}" {sökväg2}
```

### Argument
Sökvägar i formerna:
- c:\...\xml
- c:\...\xml\
- c:\...\xml\xmlfil1.xml
- .\xml
- .\xml\
- .\xml\xmlfil1.xml

utan eller inom situationstecken
- "c:\sökväg med mellanrum\xml"
- c:\sökväg_utan__mellanrum\xml

## Output
xsd-fil relativt där programmet körs ("körningskatalog").


## GIT
```bash
dotnet new gitignore
```
### GitHub
Om push remote till GitHub.
1. Skapa lokal branch, ```git branch -M main```
2. Skapa tomt repo på GitHub
3. Lokalt ```git add origin {sökväg till repo}.git```
4. Lokalt ```git push -u origin main```