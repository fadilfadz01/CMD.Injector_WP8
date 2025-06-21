:: ///////////////////////////////////////////////////
:: Snapper Records
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

set Amount=%1
set Delay=%2
set LocalFolder=%3
set ClipsFolder=%4
set Num=0

del "%LocalFolder%\RecordStop.txt"

:Loop
set /a Num=%Num%+1
if exist "%LocalFolder%\RecordStop.txt" goto Stop
for /f "tokens=*" %%a in ('tlist.exe -v ^| find.exe /c /i "App.AppX2v1fywv28r435ws9hv5t9y39fhe6jg09.mca"') do if %%a leq 2 goto Stop
sleep.exe %Delay%
start CaptureScreenApp.exe "%ClipsFolder%\SnapperShot_%Num%.jpg"
if %Num%==%Amount% (goto Stop) else (goto Loop)

:Stop
del "%LocalFolder%\RecordStop.txt"
goto :EOF