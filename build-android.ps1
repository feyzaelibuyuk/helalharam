param([switch]$Run, [switch]$Clean, [switch]$TestPackage)
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
    if ($TestPackage) { $arguments += '-p:TestBuild=true' }
    if ($Run) { $arguments += '-t:Run' }
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) { throw 'Android derlemesi başarısız.' }
    if ($TestPackage) {
        $apk = Join-Path $projectRoot 'helalharam\bin\Debug\net10.0-android\com.companyname.helalharam-Signed.apk'
        if (!(Test-Path -LiteralPath $apk)) { throw 'İmzalı test APK dosyası bulunamadı.' }
        $output = Join-Path $projectRoot 'artifacts\android-test'
        New-Item -ItemType Directory -Path $output -Force | Out-Null
        $target = Join-Path $output 'HelalRehber-0.1.0-test.apk'
        Copy-Item -LiteralPath $apk -Destination $target -Force
        Copy-Item -LiteralPath (Join-Path $projectRoot 'docs\ANDROID_TEST.md') -Destination (Join-Path $output 'KURULUM.md') -Force
        $hash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant()
        Set-Content -LiteralPath (Join-Path $output 'SHA256.txt') -Value "$hash  HelalRehber-0.1.0-test.apk" -Encoding UTF8
        Write-Host "Test paketi hazır: $target"
    }
}
finally {
    $env:DOTNET_CLI_HOME = $previousCliHome
    & subst.exe R: /D
}
