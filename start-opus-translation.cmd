@echo off
cd /d "%~dp0"
if not exist ".verification\opus-env\Scripts\python.exe" (
 echo OPUS ortami bulunamadi. docs\OPUS_ENTEGRASYON.md dosyasina bakin.
 pause
 exit /b 1
)
set PYTHONUNBUFFERED=1
set PYTHONIOENCODING=utf-8
echo Ucretsiz yerel OPUS EN-TR motoru. Bu pencere acik kalmali.
echo Diger dil ciftleri icin LibreTranslate ayrica acik olabilir.
".verification\opus-env\Scripts\python.exe" tools\LocalTranslation\opus_server.py
pause
