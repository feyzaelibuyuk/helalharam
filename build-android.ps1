param([switch]$Run)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
if (Test-Path 'R:\') { throw 'R: sürücüsü kullanımda. Önce farklı bir sürücü harfi seçin.' }
& subst.exe R: $projectRoot
if ($LASTEXITCODE -ne 0) { throw 'Geçici sürücü oluşturulamadı.' }
$previousCliHome = $env:DOTNET_CLI_HOME
try {
    $env:DOTNET_CLI_HOME = 'R:\.local-dotnet'
    $arguments = @('build', 'R:\helalharam\helalharam.csproj', '-f', 'net10.0-android', '-p:TargetFrameworks=net10.0-android', '-p:EmbedAssembliesIntoApk=true', '-p:UseSharedCompilation=false')
    if ($Run) { $arguments += '-t:Run' }
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) { throw 'Android derlemesi başarısız.' }
}
finally {
    $env:DOTNET_CLI_HOME = $previousCliHome
    & subst.exe R: /D
}


