:: ///////////////////////////////////////////////////
:: Non System-Wide
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector_WP8
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

set "InstalledLocation=%SystemDrive%\Data\PROGRAMS\{87611e4e-f9f7-411b-aca9-d24a1e001ab0}\Install"
if not exist "%InstalledLocation%" for /f %%a in ('dir %SystemDrive%\data\programs\windowsapps\CMDInjector_*_arm__kqyng60eng17c /b') do set "InstalledLocation=%SystemDrive%\data\programs\windowsapps\%%a"

if exist "%InstalledLocation%" (
	start telnetd.exe cmd.exe 9999
	start cmd.exe /c %SystemRoot%\System32\Startup.bat

	if exist "%SystemRoot%\System32\CMDInjectorFirstLaunch.dat" del "%SystemRoot%\System32\CMDInjectorFirstLaunch.dat"
	if exist "%SystemRoot%\System32\CMDInjectorTempSetup.dat" del "%SystemRoot%\System32\CMDInjectorTempSetup.dat"
	if exist "%SystemRoot%\System32\CMDInjector.dat" del "%SystemRoot%\System32\CMDInjector.dat"
)

if exist "%SystemRoot%\System32\CMDUninjector.dat" (
	rd /q /s "%SystemRoot%\System32\Dism"
	del "%SystemRoot%\System32\CMDInjectorVersion.dat"
    del "%SystemRoot%\System32\en-US\attrib.exe.mui"
    del "%SystemRoot%\System32\en-US\CheckNetIsolation.exe.mui"
    del "%SystemRoot%\System32\en-US\Dism.exe.mui"
    del "%SystemRoot%\System32\en-US\findstr.exe.mui"
    del "%SystemRoot%\System32\en-US\finger.exe.mui"
    del "%SystemRoot%\System32\en-US\help.exe.mui"
    del "%SystemRoot%\System32\en-US\hostname.exe.mui"
    del "%SystemRoot%\System32\en-US\ICacls.exe.mui"
    del "%SystemRoot%\System32\en-US\ipconfig.exe.mui"
    del "%SystemRoot%\System32\en-US\mountvol.exe.mui"
    del "%SystemRoot%\System32\en-US\neth.dll.mui"
    del "%SystemRoot%\System32\en-US\nslookup.exe.mui"
    del "%SystemRoot%\System32\en-US\ping.exe.mui"
    del "%SystemRoot%\System32\en-US\powercfg.exe.mui"
    del "%SystemRoot%\System32\en-US\regsvr32.exe.mui"
    del "%SystemRoot%\System32\en-US\sc.exe.mui"
    del "%SystemRoot%\System32\en-US\setx.exe.mui"
    del "%SystemRoot%\System32\a32.dll"
    del "%SystemRoot%\System32\attrib.exe"
    del "%SystemRoot%\System32\certmgr.exe"
    del "%SystemRoot%\System32\CheckNetIsolation.exe"
    del "%SystemRoot%\System32\comp.exe"
    del "%SystemRoot%\System32\convert.exe"
    del "%SystemRoot%\System32\chkdsk.exe"
    del "%SystemRoot%\System32\dcopy.exe"
    del "%SystemRoot%\System32\depends.exe"
    del "%SystemRoot%\System32\DIALTESTWP8.EXE"
    del "%SystemRoot%\System32\Dism.exe"
    del "%SystemRoot%\System32\doskey.exe"
    del "%SystemRoot%\System32\fc.exe"
    del "%SystemRoot%\System32\find.exe"
    del "%SystemRoot%\System32\findstr.exe"
    del "%SystemRoot%\System32\finger.exe"
    del "%SystemRoot%\System32\ftpd.exe"
    del "%SystemRoot%\System32\help.exe"
    del "%SystemRoot%\System32\HOSTNAME.EXE"
    del "%SystemRoot%\System32\icacls.exe"
    del "%SystemRoot%\System32\ipconfig.exe"
    del "%SystemRoot%\System32\k32.dll"
    del "%SystemRoot%\System32\kill.exe"
    del "%SystemRoot%\System32\label.exe"
    del "%SystemRoot%\System32\minshutdown.exe"
    del "%SystemRoot%\System32\more.com"
    del "%SystemRoot%\System32\mwkdbgctrl.exe"
    del "%SystemRoot%\System32\mountvol.exe"
    del "%SystemRoot%\System32\msnap.exe"
    del "%SystemRoot%\System32\net.exe"
    del "%SystemRoot%\System32\net1.exe"
    del "%SystemRoot%\System32\neth.dll"
    del "%SystemRoot%\System32\netsh.exe"
    del "%SystemRoot%\System32\nslookup.exe"
    del "%SystemRoot%\System32\ping.exe"
    del "%SystemRoot%\System32\powercfg.exe"
    del "%SystemRoot%\System32\ProvisioningTool.exe"
    del "%SystemRoot%\System32\regini.exe"
    del "%SystemRoot%\System32\regsvr32.exe"
    del "%SystemRoot%\System32\sc.exe"
    del "%SystemRoot%\System32\setx.exe"
    del "%SystemRoot%\System32\SirepController.exe"
    del "%SystemRoot%\System32\sleep.exe"
    del "%SystemRoot%\System32\tlist.exe"
    del "%SystemRoot%\System32\tracelog.exe"
    del "%SystemRoot%\System32\WPConPlatDev.exe"
	del "%SystemRoot%\System32\CMDUninjector.dat"
)
goto :EOF