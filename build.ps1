[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string[]]$TodoSources = @(),

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

try {
    $projectRoot = $PSScriptRoot
    $todoRoot = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'src/todo'))
    $buildRoot = Join-Path $projectRoot 'build/project'
    $resultRoot = Join-Path $projectRoot 'build/result'

    if ($TodoSources.Count -eq 0) {
        $sources = @(Get-ChildItem -LiteralPath $todoRoot -Filter '*.cpp' -File -Recurse |
            Sort-Object FullName | ForEach-Object { $_.FullName })
    }
    else {
        $sources = @(
            foreach ($source in $TodoSources) {
                $path = $source
                if (-not [System.IO.Path]::IsPathRooted($path)) {
                    if ($path -match '^src[\\/]todo[\\/]') {
                        $path = Join-Path $projectRoot $path
                    }
                    else {
                        $path = Join-Path $todoRoot $path
                    }
                }
                $resolved = (Resolve-Path -LiteralPath $path).ProviderPath
                $prefix = $todoRoot + [System.IO.Path]::DirectorySeparatorChar
                if (-not $resolved.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase) -or
                    [System.IO.Path]::GetExtension($resolved) -ne '.cpp' -or
                    -not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
                    Write-Error "Select only .cpp files inside src/todo: $source"
                }
                $resolved
            }
        )
        $sources = @($sources | Sort-Object -Unique)
    }
    if ($sources.Count -eq 0) {
        Write-Error 'No todo source files were selected.'
    }

    $cmakeCommand = Get-Command cmake -ErrorAction SilentlyContinue
    if ($cmakeCommand) {
        $cmake = $cmakeCommand.Source
    }
    else {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
        if (-not (Test-Path -LiteralPath $vswhere)) {
            Write-Error 'Install Visual Studio C++ desktop development and CMake tools.'
        }
        $cmake = & $vswhere -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
            -find 'Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe' |
            Select-Object -First 1
        if (-not $cmake -or -not (Test-Path -LiteralPath $cmake)) {
            Write-Error 'CMake was not found. Install Visual Studio CMake tools.'
        }
    }

    Write-Host "Building $Configuration with $($sources.Count) todo sources..."
    $sourceList = ($sources | ForEach-Object { $_.Replace('\', '/') }) -join ';'
    & $cmake -S $projectRoot -B $buildRoot -A x64 "-DSTUDY_TODO_SOURCES:STRING=$sourceList"
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
    & $cmake --build $buildRoot --config $Configuration --target StudyGame
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    $outputRoot = Join-Path $buildRoot "bin/$Configuration"
    $executable = Join-Path $outputRoot 'StudyGame.exe'
    if (-not (Test-Path -LiteralPath $executable)) {
        Write-Error "Build output not found: $executable"
    }
    New-Item -ItemType Directory -Path $resultRoot -Force | Out-Null
    Copy-Item -LiteralPath $executable -Destination $resultRoot -Force
    Get-ChildItem -LiteralPath $outputRoot -Filter '*.dll' -File |
        Copy-Item -Destination $resultRoot -Force
    Copy-Item -LiteralPath (Join-Path $outputRoot 'content') -Destination $resultRoot -Recurse -Force

    Write-Host "Build complete: $(Join-Path $resultRoot 'StudyGame.exe')" -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
