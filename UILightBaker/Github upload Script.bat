@echo off
title Github Comitter

cd %~dp0
for /F "tokens=1,2" %%i in (param.txt) do call :process %%i %%j
goto thenextstep
:process
set MESSAGE=%1
set SHUTDOWN=%2

git branch
git fetch
git add -A
git commit -m %MESSAGE%
git push


if "%SHUTDOWN%"=="True" (
	shutdown /s /f /t 10
)

if "%SHUTDOWN%"=="False" (
	pause
)