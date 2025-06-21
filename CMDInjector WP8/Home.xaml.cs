using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using CMDInjectorHelper;
using System.Threading.Tasks;
using Windows.Storage;
using CMDInjector_WP8.Resources;

namespace CMDInjector_WP8
{
    public partial class Home : PhoneApplicationPage
    {
        public Home()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (File.Exists(@"C:\Windows\System32\CMDInjector.dat") || File.Exists(@"C:\Windows\System32\CMDUninjector.dat"))
                {
                    InjectBtn.IsEnabled = false;
                    UnInjectBtn.IsEnabled = false;
                    if (File.Exists(@"C:\Windows\System32\CMDInjector.dat"))
                    {
                        InjectBtn.Content = AppResources.HomePageInjectButtonText2;
                        reInjectionReboot.Text = AppResources.HomePageInectionRebootDescription1;
                    }
                    else
                    {
                        UnInjectBtn.Content = AppResources.HomePageUninjectButtonText2;
                        reInjectionReboot.Text = AppResources.HomePageUninectionRebootDescription1;
                    }
                    UnInjectBtn.Visibility = Visibility.Visible;
                    reInjectionReboot.Visibility = Visibility.Visible;
                }
                else if (HomeHelper.IsCMDInjected() && File.Exists(@"C:\Windows\System32\CMDInjectorVersion.dat"))
                {
                    if (Convert.ToInt32(Helper.InjectedBatchVersion) > Helper.currentBatchVersion)
                    {
                        InjectBtn.IsEnabled = false;
                        InjectBtn.Content = AppResources.HomePageInjectButtonText2;
                        reInjectionNote.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (Convert.ToInt32(Helper.InjectedBatchVersion) < Helper.currentBatchVersion)
                        {
                            reInjectionBox.Visibility = Visibility.Visible;
                        }
                        InjectBtn.Content = AppResources.HomePageInjectButtonText3;
                    }
                    UnInjectBtn.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async void InjectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InjectBtn.IsEnabled = false;
                UnInjectBtn.IsEnabled = false;
                InjectBtn.Content = AppResources.HomePageInjectButtonText4;
                await Task.Run(async () =>
                {
                    await FileIO.WriteTextAsync(await Helper.localFolder.CreateFileAsync("CMDInjector.dat", CreationCollisionOption.ReplaceExisting), Helper.currentBatchVersion.ToString());
                    Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjector.dat");
                    await Task.Delay(100);
                    Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjectorVersion.dat");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Setup.bat", @"C:\Windows\System32\CMDInjectorSetup.bat");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\SystemWide.bat", @"C:\Windows\System32\CMDInjector.bat");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Bootsh\\bootshsvc.dll", @"C:\Windows\System32\bootshsvc.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Bootsh\\bootshsvc.dll.mui", @"C:\Windows\System32\en-US\bootshsvc.dll.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Startup\\startup.bsc", @"C:\Windows\System32\Boot\startup.bsc");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\attrib.exe.mui", @"C:\Windows\System32\en-US\attrib.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\bcdedit.exe.mui", @"C:\Windows\System32\en-US\bcdedit.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\CheckNetIsolation.exe.mui", @"C:\Windows\System32\en-US\CheckNetIsolation.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\cmd.exe.mui", @"C:\Windows\System32\en-US\cmd.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\Dism.exe.mui", @"C:\Windows\System32\en-US\Dism.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\findstr.exe.mui", @"C:\Windows\System32\en-US\findstr.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\finger.exe.mui", @"C:\Windows\System32\en-US\finger.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\help.exe.mui", @"C:\Windows\System32\en-US\help.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\hostname.exe.mui", @"C:\Windows\System32\en-US\hostname.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\ICacls.exe.mui", @"C:\Windows\System32\en-US\ICacls.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\ipconfig.exe.mui", @"C:\Windows\System32\en-US\ipconfig.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\mountvol.exe.mui", @"C:\Windows\System32\en-US\mountvol.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\neth.dll.mui", @"C:\Windows\System32\en-US\neth.dll.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\nslookup.exe.mui", @"C:\Windows\System32\en-US\nslookup.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\ping.exe.mui", @"C:\Windows\System32\en-US\ping.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\powercfg.exe.mui", @"C:\Windows\System32\en-US\powercfg.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\reg.exe.mui", @"C:\Windows\System32\en-US\reg.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\regsvr32.exe.mui", @"C:\Windows\System32\en-US\regsvr32.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\sc.exe.mui", @"C:\Windows\System32\en-US\sc.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\setx.exe.mui", @"C:\Windows\System32\en-US\setx.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\a32.dll", @"C:\Windows\System32\a32.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\attrib.exe", @"C:\Windows\System32\attrib.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\bcdedit.exe", @"C:\Windows\System32\bcdedit.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\CaptureScreenApp.exe", @"C:\Windows\System32\CaptureScreenApp.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\certmgr.exe", @"C:\Windows\System32\certmgr.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\CheckNetIsolation.exe", @"C:\Windows\System32\CheckNetIsolation.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\cmd.exe", @"C:\Windows\System32\cmd.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\comp.exe", @"C:\Windows\System32\comp.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\chkdsk.exe", @"C:\Windows\System32\chkdsk.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\dcopy.exe", @"C:\Windows\System32\dcopy.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\depends.exe", @"C:\Windows\System32\depends.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\DIALTESTWP8.EXE", @"C:\Windows\System32\DIALTESTWP8.EXE");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\Dism.exe", @"C:\Windows\System32\Dism.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\doskey.exe", @"C:\Windows\System32\doskey.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\fc.exe", @"C:\Windows\System32\fc.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\find.exe", @"C:\Windows\System32\find.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\findstr.exe", @"C:\Windows\System32\findstr.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\finger.exe", @"C:\Windows\System32\finger.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ftpd.exe", @"C:\Windows\System32\ftpd.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\gbot.exe", @"C:\Windows\System32\gbot.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\gse.dll", @"C:\Windows\System32\gse.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\help.exe", @"C:\Windows\System32\help.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\HOSTNAME.EXE", @"C:\Windows\System32\HOSTNAME.EXE");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\icacls.exe", @"C:\Windows\System32\icacls.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ipconfig.exe", @"C:\Windows\System32\ipconfig.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\k32.dll", @"C:\Windows\System32\k32.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\kill.exe", @"C:\Windows\System32\kill.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\label.exe", @"C:\Windows\System32\label.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\minshutdown.exe", @"C:\Windows\System32\minshutdown.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\more.com", @"C:\Windows\System32\more.com");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\mwkdbgctrl.exe", @"C:\Windows\System32\mwkdbgctrl.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\mountvol.exe", @"C:\Windows\System32\mountvol.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\msnap.exe", @"C:\Windows\System32\msnap.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\net.exe", @"C:\Windows\System32\net.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\net1.exe", @"C:\Windows\System32\net1.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\neth.dll", @"C:\Windows\System32\neth.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\netsh.exe", @"C:\Windows\System32\netsh.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\nslookup.exe", @"C:\Windows\System32\nslookup.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\pacman_ierror.dll", @"C:\Windows\System32\pacman_ierror.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\pacmanerr.dll", @"C:\Windows\System32\pacmanerr.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ping.exe", @"C:\Windows\System32\ping.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\powercfg.exe", @"C:\Windows\System32\powercfg.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\PowerTool.exe", @"C:\Windows\System32\PowerTool.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ProvisioningTool.exe", @"C:\Windows\System32\ProvisioningTool.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\reg.exe", @"C:\Windows\System32\reg.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\regini.exe", @"C:\Windows\System32\regini.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\regsvr32.exe", @"C:\Windows\System32\regsvr32.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sc.exe", @"C:\Windows\System32\sc.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ScreenSnapper.exe", @"C:\Windows\System32\ScreenSnapper.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\setx.exe", @"C:\Windows\System32\setx.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\shutdown.exe", @"C:\Windows\System32\shutdown.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\SirepController.exe", @"C:\Windows\System32\SirepController.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sleep.exe", @"C:\Windows\System32\sleep.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\SlideToShutdown.exe", @"C:\Windows\System32\SlideToShutdown.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TaskView.exe", @"C:\Windows\System32\TaskView.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\telnetd.exe", @"C:\Windows\System32\telnetd.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TestDeploymentInfo.dll", @"C:\Windows\System32\TestDeploymentInfo.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\th.dll", @"C:\Windows\System32\th.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TH.exe", @"C:\Windows\System32\TH.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\tlist.exe", @"C:\Windows\System32\tlist.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\tracelog.exe", @"C:\Windows\System32\tracelog.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\VolumeDown.exe", @"C:\Windows\System32\VolumeDown.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\VolumeUp.exe", @"C:\Windows\System32\VolumeUp.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\WPConPlatDev.exe", @"C:\Windows\System32\WPConPlatDev.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\xcopy.exe", @"C:\Windows\System32\xcopy.exe");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\SecurityManager\\PrincipalClasses\\PRINCIPAL_CLASS_TCB", "Directories", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "C:\\ ");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "Type", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000010");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000002");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "ServiceSidType", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "ErrorControl", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                    Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "ImagePath", Helper.RegistryHelper.RegistryType.REG_EXPAND_SZ, "%SystemRoot%\\system32\\svchost.exe -k Bootshsvc");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "DisplayName", Helper.RegistryHelper.RegistryType.REG_SZ, "@bootshsvc.dll,-1");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "Description", Helper.RegistryHelper.RegistryType.REG_SZ, "@bootshsvc.dll,-2");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "ObjectName", Helper.RegistryHelper.RegistryType.REG_SZ, "LocalSystem");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "DependOnService", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "Afd lmhosts keyiso ");
                    Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "FailureActions", Helper.RegistryHelper.RegistryType.REG_BINARY, "80510100000000000000000003000000140000000100000060EA00000100000060EA00000000000000000000");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "RequiredPrivileges", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "SeAssignPrimaryTokenPrivilege SeAuditPrivilege SeSecurityPrivilege SeChangeNotifyPrivilege SeCreateGlobalPrivilege SeDebugPrivilege SeImpersonatePrivilege SeIncreaseQuotaPrivilege SeTcbPrivilege SeBackupPrivilege SeRestorePrivilege SeShutdownPrivilege SeSystemProfilePrivilege SeSystemtimePrivilege SeManageVolumePrivilege SeCreatePagefilePrivilege SeCreatePermanentPrivilege SeCreateSymbolicLinkPrivilege SeIncreaseBasePriorityPrivilege SeIncreaseWorkingSetPrivilege SeLoadDriverPrivilege SeLockMemoryPrivilege SeProfileSingleProcessPrivilege SeSystemEnvironmentPrivilege SeTakeOwnershipPrivilege SeTimeZonePrivilege ");
                    Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters", "ServiceDll", Helper.RegistryHelper.RegistryType.REG_EXPAND_SZ, "%SystemRoot%\\system32\\bootshsvc.dll");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters", "ServiceDllUnloadOnStop", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                    /*Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters\\Commands", "Loopback", 1, "CheckNetIsolation.exe loopbackexempt -a -n=CMDInjector_kqyng60eng17c");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters\\Commands", "Telnetd", 1, "telnetd.exe cmd.exe 9999");*/
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Svchost", "bootshsvc", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "bootsh ");
                });
                reInjectionReboot.Text = AppResources.HomePageInectionRebootDescription2;
                reInjectionBox.Visibility = Visibility.Collapsed;
                reInjectionReboot.Visibility = Visibility.Visible;
                InjectBtn.Content = AppResources.HomePageInjectButtonText2;
                Reboot();
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private async void UnInjectBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await Helper.DisplayMessage2(AppResources.HomePageUninjectionWarningDescription, Helper.SoundHelper.Sound.Alert, AppResources.HomePageUninjectionWarningTitle, AppResources.HomePageUninjectionWarningButton2, true, AppResources.HomePageUninjectionWarningButton1);
            if (result == 0)
            {
                InjectBtn.IsEnabled = false;
                UnInjectBtn.IsEnabled = false;
                await FileIO.WriteTextAsync(await Helper.localFolder.CreateFileAsync("CMDInjector.dat", CreationCollisionOption.ReplaceExisting), Helper.currentBatchVersion.ToString());
                Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDUninjector.dat");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\NonSystemWide.bat", @"C:\Windows\System32\CMDInjector.bat");
                UnInjectBtn.Content = AppResources.HomePageUninjectButtonText2;
                reInjectionReboot.Text = AppResources.HomePageUninectionRebootDescription2;
                reInjectionBox.Visibility = Visibility.Collapsed;
                reInjectionNote.Visibility = Visibility.Collapsed;
                reInjectionReboot.Visibility = Visibility.Visible;
                Reboot();
            }
        }

        private async void Reboot()
        {
            try
            {
                var res = await Helper.DisplayMessage2(AppResources.HomePageRebootDescription, Helper.SoundHelper.Sound.Alert, AppResources.HomePageRebootTitle, AppResources.HomePageRebootButton2, true, AppResources.HomePageRebootButton1);
                if (res == 0)
                {
                    Helper.RebootSystem();
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void FaqHelp_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Help.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}