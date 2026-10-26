@echo off
cd /d "%~dp0"
if not exist ".local-translate\Scripts\libretranslate.exe" (
  echo Yerel ceviri motoru kurulu degil. docs\ETIKET_MENU_CEVIRI.md dosyasina bakin.
  pause
  exit /b 1
)
set PYTHONUNBUFFERED=1
set PYTHONIOENCODING=utf-8
echo Ucretsiz yerel ceviri. Ilk acilista EN/TR modelleri indirilir.
echo Bu pencere acik kalmali. Durdurmak icin Ctrl+C.
".local-translate\Scripts\python.exe" tools\LocalTranslation\server.py --host 127.0.0.1 --port 5000 --load-only en,tr --char-limit 12000
pause
