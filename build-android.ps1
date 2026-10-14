param([switch]$Run, [switch]$Clean)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
if (Test-Path 'R:\') { throw 'R: sürücüsü kullanımda. Önce farklı bir sürücü harfi seçin.' }
if ($Clean) {
    $artifactPath = [IO.Path]::GetFullPath((Join-Path $projectRoot 'helalharam\obj'))
    if (Test-Path -LiteralPath $artifactPath) {
        $item = Get-Item -LiteralPath $artifactPath
        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 -or
            (Resolve-Path -LiteralPath $artifactPath).Path -ne $artifactPath) { throw 'Derleme klasörü doğrulanamadı.' }
        Remove-Item -LiteralPath $artifactPath -Recurse -Force
    }
}
& subst.exe R: $projectRoot
if ($LASTEXITCODE -ne 0) { throw 'Geçici sürücü oluşturulamadı.' }
$previousCliHome = $env:DOTNET_CLI_HOME
try {
    $env:DOTNET_CLI_HOME = Join-Path $projectRoot '.local-dotnet'
    $arguments = @('build', 'R:\helalharam\helalharam.csproj', '-f', 'net10.0-android', '-p:TargetFrameworks=net10.0-android', '-p:EmbedAssembliesIntoApk=true', '-p:UseSharedCompilation=false')
    if ($Run) { $arguments += '-t:Run' }
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) { throw 'Android derlemesi başarısız.' }
}
finally {
    $env:DOTNET_CLI_HOME = $previousCliHome
    & subst.exe R: /D
}
