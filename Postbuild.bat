@echo off

:: Close the processes if they are running
taskkill /F /IM "Snake Launcher.exe" 2>nul
taskkill /F /IM "Snake.exe" 2>nul

echo Processes closed.

:: Define source and destination directories
set "SOURCE1=C:\Users\terry\source\repos\Snake\Snake\bin\Debug\net8.0-windows"
set "SOURCE2=C:\Users\terry\source\repos\Snake\Snake Launcher\bin\Debug\net8.0-windows"
set "DESTINATION1=C:\Users\terry\Documents\Snake Launcher\Game"
set "DESTINATION2=C:\Users\terry\Documents\Snake Launcher"

:: Ensure the destination directories exist
if not exist "%DESTINATION1%" mkdir "%DESTINATION1%"
if not exist "%DESTINATION2%" mkdir "%DESTINATION2%"

:: Copy files from bin to Snake
xcopy "%SOURCE1%\*" "%DESTINATION1%" /E /I /Y
xcopy "%SOURCE2%\*" "%DESTINATION2%" /E /I /Y

echo Files copied successfully.

pause
