$ErrorActionPreference = 'Stop'
$setupProject = Join-Path $PSScriptRoot 'tools\AdminSetup\AdminSetup.csproj'
& dotnet build $setupProject --nologo
if ($LASTEXITCODE -ne 0) { throw 'Yönetici parola aracı derlenemedi.' }
& dotnet (Join-Path $PSScriptRoot 'tools\AdminSetup\bin\Debug\net10.0\AdminSetup.dll') (Join-Path $PSScriptRoot 'BlazorApp1\BlazorApp1.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Yönetici parolası ayarlanmadı.' }
