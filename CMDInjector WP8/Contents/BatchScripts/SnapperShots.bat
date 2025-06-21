:: ///////////////////////////////////////////////////
:: Snapper Shots
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

set Amount=%1
set Delay=%2
set LocalFolder=%3
set Num=0

del "%LocalFolder%\CaptureStop.txt"

:Loop
set /a Num=%Num%+1
sleep.exe %Delay%
if exist "%LocalFolder%\CaptureStop.txt" goto Stop
for /f "tokens=*" %%a in ('tlist.exe -v ^| find.exe /c /i "App.AppX2v1fywv28r435ws9hv5t9y39fhe6jg09.mca"') do if %%a leq 2 goto Stop
ScreenSnapper.exe
if %Num%==%Amount% (goto Stop) else (goto Loop)

:Stop
del "%LocalFolder%\CaptureStop.txt"
goto :EOF