@echo off
set source1="C:\Users\61805216CTC\source\repos\Snake Launcher\Snake Launcher\bin\Debug\net8.0-windows"
set destination1="C:\Users\61805216CTC\Documents\Snake + Launcher"
set source2="C:\Users\61805216CTC\source\repos\Snake\Snake\bin\Debug\net8.0-windows"
set destination2="C:\Users\61805216CTC\Documents\Snake + Launcher\Game"

REM Kill all old processes
taskkill /f /im "Snake.exe"
taskkill /f /im "Snake Launcher.exe"

REM Ensure the destination directory exists
powershell -Command "if (!(Test-Path -Path '%destination1%')) { New-Item -ItemType Directory -Path '%destination1%' }"
powershell -Command "if (!(Test-Path -Path '%destination2%')) { New-Item -ItemType Directory -Path '%destination2%' }"

REM Copy all files from the source to the destination
powershell -Command "Copy-Item -Path '%source1%\*' -Destination '%destination1%' -Recurse -Force"
powershell -Command "Copy-Item -Path '%source2%\*' -Destination '%destination2%' -Recurse -Force"

echo Files copied successfully.
pause