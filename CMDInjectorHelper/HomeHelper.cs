﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDInjectorHelper
{
    public static class HomeHelper
    {
        public static bool IsCMDInjected()
        {
            if (File.Exists(@"C:\Windows\System32\Boot\startup.bsc") && File.Exists(@"C:\Windows\System32\en-US\bootshsvc.dll.mui") && File.Exists(@"C:\Windows\System32\en-US\bcdedit.exe.mui") &&
                File.Exists(@"C:\Windows\System32\VolumeUp.exe") && File.Exists(@"C:\Windows\System32\en-US\cmd.exe.mui") && File.Exists(@"C:\Windows\System32\en-US\reg.exe.mui") &&
                File.Exists(@"C:\Windows\System32\bootshsvc.dll") && File.Exists(@"C:\Windows\System32\bcdedit.exe") && //File.Exists(@"C:\Windows\System32\CheckNetIsolation.exe") &&
                File.Exists(@"C:\Windows\System32\cmd.exe") && File.Exists(@"C:\Windows\System32\TH.exe") && File.Exists(@"C:\Windows\System32\tlist.exe") &&
                File.Exists(@"C:\Windows\System32\PowerTool.exe") && File.Exists(@"C:\Windows\System32\reg.exe") && File.Exists(@"C:\Windows\System32\CaptureScreenApp.exe") &&
                File.Exists(@"C:\Windows\System32\shutdown.exe") && File.Exists(@"C:\Windows\System32\telnetd.exe") && File.Exists(@"C:\Windows\System32\ScreenSnapper.exe") &&
                /*File.Exists(@"C:\Windows\System32\ICacls.exe") &&*/ File.Exists(@"C:\Windows\System32\VolumeDown.exe") && File.Exists(@"C:\Windows\System32\sleep.exe") &&
                File.Exists(@"C:\Windows\System32\find.exe") && File.Exists(@"C:\Windows\System32\CMDInjectorSetup.bat") && File.Exists(@"C:\Windows\System32\k32.dll") &&
                /*File.Exists(@"C:\Windows\System32\en-US\sort.exe.mui") &&*/ File.Exists(@"C:\Windows\System32\xcopy.exe") && File.Exists(@"C:\Windows\System32\TestDeploymentInfo.dll") &&
                File.Exists(@"C:\Windows\System32\pacman_ierror.dll") && File.Exists(@"C:\Windows\System32\pacmanerr.dll") && File.Exists(@"C:\Windows\System32\th.dll"))
            {
                return true;
            }
            return false;
        }

        public static string GetTelnetTroubleshoot()
        {
            if (!File.Exists(@"C:\Windows\System32\Boot\startup.bsc") || !File.Exists(@"C:\Windows\System32\cmd.exe") || !File.Exists(@"C:\Windows\System32\telnetd.exe"))
            {
                return "Make sure you have restored NDTKSvc and reboot the device.";
            }
            else if (File.Exists(@"C:\Windows\System32\Boot\startup.bsc") && !string.Equals(new StreamReader(@"C:\Windows\System32\Boot\startup.bsc").ReadToEnd(), new StreamReader($"{Helper.installedLocation.Path}\\Contents\\Startup\\startup.bsc").ReadToEnd()))
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Startup\\startup.bsc", @"C:\Windows\System32\Boot\startup.bsc");
                return "The Bootsh service component has manually changed, or corrupted. Please reboot the device to fix it.";
            }
            else if (File.Exists(@"C:\Windows\System32\CMDInjectorFirstLaunch.dat"))
            {
                return "The system isn't rebooted to initialize the App after the first launch, please reboot the device.";
            }
            else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000004" && Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000000")
            {
                return "The Bootsh service & UMCIAuditMode is disabled. Please enable it from the App settings and reboot the device.";
            }
            else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000004")
            {
                return "The Bootsh service is disabled. Please enable it from the App settings and reboot the device.";
            }
            else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000000")
            {
                return "The UMCIAuditMode is disabled. Please enable it from the App settings and reboot the device.";
            }
            else
            {
                return "Something went wrong, try restarting the App or the device.";
            }
        }

        public static bool IsConnected()
        {
            if (!File.Exists(@"C:\Windows\System32\Boot\startup.bsc") || !File.Exists(@"C:\Windows\System32\cmd.exe") || !File.Exists(@"C:\Windows\System32\telnetd.exe"))
            {
                return false;
            }
            else if (File.Exists(@"C:\Windows\System32\Boot\startup.bsc") && !string.Equals(new StreamReader(@"C:\Windows\System32\Boot\startup.bsc").ReadToEnd(), new StreamReader($"{Helper.installedLocation.Path}\\Contents\\Startup\\startup.bsc").ReadToEnd()))
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Startup\\startup.bsc", @"C:\Windows\System32\Boot\startup.bsc");
                return false;
            }
            else if (File.Exists(@"C:\Windows\System32\CMDInjectorFirstLaunch.dat"))
            {
                return false;
            }
            else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000004" && Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000000")
            {
                return false;
            }
            else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000004")
            {
                return false;
            }
            else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000000")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
