#!/bin/bash
# Script para ejecutar tests con cobertura de código

echo "🧪 Ejecutando tests con cobertura de código..."

# Limpiar resultados anteriores
if [ -d "TestResults" ]; then
    rm -rf TestResults
    echo "🗑️ Limpieza de resultados anteriores completada"
fi

# Crear directorio para resultados
mkdir -p TestResults

echo "🔄 Ejecutando tests y recolectando cobertura..."

# Ejecutar tests con cobertura SIN threshold (para evitar error de compilación)
dotnet test tests/MathRacerAPI.Tests/MathRacerAPI.Tests.csproj \
    --configuration Release \
    --verbosity normal \
    --collect:"XPlat Code Coverage" \
    --results-directory TestResults \
    --logger "console;verbosity=minimal" \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=TestResults/coverage.cobertura.xml \
    /p:Include="[MathRacerAPI.Domain]*" \
    /p:Exclude="[MathRacerAPI.Domain]*.Program"

# Verificar si los tests pasaron
if [ $? -ne 0 ]; then
    echo "❌ Los tests fallaron durante la ejecución"
    exit 1
fi

echo "✅ Tests ejecutados correctamente"

# Los archivos de cobertura se buscan más adelante en el proceso

# Buscar archivo de cobertura
COVERAGE_FILES=$(find TestResults -name "coverage.cobertura.xml" 2>/dev/null)
THRESHOLD_MET=false

if [ -n "$COVERAGE_FILES" ]; then
    COVERAGE_FILE=$(echo "$COVERAGE_FILES" | head -1)
    echo "📊 Archivo encontrado: $COVERAGE_FILE"
    
    # Analizar cobertura del Domain usando xmllint si está disponible  
    if command -v xmllint &> /dev/null; then
        DOMAIN_LINE_RATE=$(xmllint --xpath "string(//package[@name='MathRacerAPI.Domain']/@line-rate)" "$COVERAGE_FILE" 2>/dev/null)
        if [ -n "$DOMAIN_LINE_RATE" ]; then
            COVERAGE_PERCENT=$(echo "$DOMAIN_LINE_RATE * 100" | bc -l | xargs printf "%.2f")
            echo "📈 Cobertura MathRacerAPI.Domain: ${COVERAGE_PERCENT}%"
            
            if (( $(echo "$DOMAIN_LINE_RATE >= 0.70" | bc -l) )); then
                echo "✅ Cobertura OK (${COVERAGE_PERCENT}% >= 70%)"
                THRESHOLD_MET=true
            else
                echo "❌ Cobertura insuficiente (${COVERAGE_PERCENT}% < 70%)"
                THRESHOLD_MET=false
            fi
        else
            echo "⚠️ No se pudo leer la cobertura del Domain en el XML"
        fi
    else
        echo "⚠️ xmllint no disponible. Instala con: apt-get install libxml2-utils (Ubuntu/Debian)"
    fi
else
    echo "❌ No se encontró archivo de cobertura"
fi

# Generar reporte HTML si ReportGenerator está disponible
if command -v reportgenerator &> /dev/null; then
    echo "📊 Generando reporte HTML de cobertura..."
    reportgenerator \
        -reports:"TestResults/coverage.cobertura.xml" \
        -targetdir:"TestResults/CoverageReport" \
        -reporttypes:Html
    
    echo "📋 Reporte HTML generado en: TestResults/CoverageReport/index.html"
else
    echo "⚠️ ReportGenerator no está instalado. Instala con: dotnet tool install --global dotnet-reportgenerator-globaltool"
fi

# Resultado final basado en el threshold
if [ "$THRESHOLD_MET" = true ]; then
    echo "🎯 ¡Cobertura de código completada exitosamente!"
    exit 0
else
    echo "🎯 Cobertura de código completada - Threshold no alcanzado"
    exit 1
fi

echo "🎯 Cobertura de código completada."