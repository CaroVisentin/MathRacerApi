# Script PowerShell para ejecutar tests con cobertura de código

Write-Host "Ejecutando tests con cobertura de codigo..." -ForegroundColor Cyan

# Limpiar resultados anteriores
if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force "TestResults"
    Write-Host "Limpieza completada" -ForegroundColor Yellow
}

# Crear directorio
New-Item -ItemType Directory -Force -Path "TestResults" | Out-Null

# Ejecutar tests
dotnet test tests/MathRacerAPI.Tests/MathRacerAPI.Tests.csproj --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=TestResults/coverage.cobertura.xml "/p:Include=[MathRacerAPI.Domain]*" "/p:Exclude=[MathRacerAPI.Domain]*.Program"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests fallaron" -ForegroundColor Red
    exit 1
}

Write-Host "Tests completados" -ForegroundColor Green

# Buscar archivo de cobertura
$coverageFiles = Get-ChildItem -Recurse -Path "TestResults" -Filter "coverage.cobertura.xml"
if ($coverageFiles.Count -gt 0) {
    $coverageFile = $coverageFiles[0].FullName
    Write-Host "Archivo de cobertura encontrado: $coverageFile" -ForegroundColor Green
    
    # Leer cobertura específica del Domain
    $xml = [xml](Get-Content $coverageFile)
    $domainPackage = $xml.coverage.packages.package | Where-Object { $_.name -eq "MathRacerAPI.Domain" }
    
    if ($domainPackage -and $domainPackage.'line-rate') {
        $lineRate = [double]$domainPackage.'line-rate'
        $percent = [math]::Round($lineRate * 100, 2)
        Write-Host "Cobertura MathRacerAPI.Domain: $percent%" -ForegroundColor Cyan
        
        if ($percent -ge 70) {
            Write-Host "Cobertura OK ($percent% >= 70%)" -ForegroundColor Green
            exit 0
        } else {
            Write-Host "Cobertura insuficiente ($percent% < 70%)" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "No se pudo leer la cobertura del Domain en el XML" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "No se encontro archivo de cobertura" -ForegroundColor Red
    exit 1
}