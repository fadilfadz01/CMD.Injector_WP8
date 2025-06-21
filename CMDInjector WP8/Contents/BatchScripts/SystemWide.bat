:: ///////////////////////////////////////////////////
:: System-Wide
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector_WP8
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

::start ftpd.exe
start telnetd.exe cmd.exe 9999
start telnetd.exe cmd.exe 23
start cmd.exe /c %SystemRoot%\System32\Startup.bat

if exist "%SystemRoot%\System32\CMDInjectorFirstLaunch.dat" del "%SystemRoot%\System32\CMDInjectorFirstLaunch.dat"
if exist "%SystemRoot%\System32\CMDInjectorTempSetup.dat" del "%SystemRoot%\System32\CMDInjectorTempSetup.dat"
if exist "%SystemRoot%\System32\CMDUninjector.dat" del "%SystemRoot%\System32\CMDUninjector.dat"

set "InstalledLocation=%SystemDrive%\Data\PROGRAMS\{87611e4e-f9f7-411b-aca9-d24a1e001ab0}\Install"
if not exist "%InstalledLocation%" for /f %%a in ('dir %SystemDrive%\data\programs\windowsapps\CMDInjector_*_arm__kqyng60eng17c /b') do set "InstalledLocation=%SystemDrive%\data\programs\windowsapps\%%a"

if exist "%InstalledLocation%" (
	if exist "%SystemRoot%\system32\CMDInjector.dat" (
		xcopy /cey "%InstalledLocation%\Contents\ConsoleApps\Dism" "%SystemRoot%\system32\Dism\"
		del "%SystemRoot%\system32\CMDInjector.dat"
	)
)
goto :EOF