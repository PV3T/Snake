@echo off

:: Close the processes if they are running
taskkill /F /IM "Snake Launcher.exe" 2>nul
taskkill /F /IM "Snake.exe" 2>nul

echo Processes closed.

:: Define source and destination directories
set "SOURCE=C:\Users\61805216CTC\source\repos\Snake\Snake\bin\Debug\net8.0-windows"
set "DESTINATION=C:\Users\61805216CTC\Documents\Snake"

:: Ensure the destination directories exist
if not exist "%DESTINATION%" mkdir "%DESTINATION%"

:: Copy files from bin to Snake
xcopy "%SOURCE%\*" "%DESTINATION%" /E /I /Y

echo Files copied successfully.

pause
