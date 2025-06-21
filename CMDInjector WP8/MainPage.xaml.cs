using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CMDInjector_WP8.Resources;
using System.Windows.Media.Imaging;
using CMDInjectorHelper;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage;
using System.ComponentModel;

namespace CMDInjector_WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool buttonOnHold = false;
        bool flag = false;

        public MainPage()
        {
            InitializeComponent();
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            Initialize();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            e.Cancel = true;
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(async () =>
                {
                    if (!flag)
                    {
                        flag = true;
                        var result = await Helper.DisplayMessage2(AppResources.MainPageExitMessageBoxContent, Helper.SoundHelper.Sound.Alert, "CMD Injector", "No", true, "Yes");
                        if (result == 0)
                        {
                            CoreApplication.Exit();
                        }
                        else
                            flag = false;
                    }
                    else
                        flag = false;
                });
            }
        }

        private async Task<bool> CheckCompatibility()
        {
            await Task.Delay(200);
            if (Environment.OSVersion.Version.Major == 10)
            {
                var result = await Helper.DisplayMessage2(AppResources.MainPageUnsupportedDescription, Helper.SoundHelper.Sound.Alert, "CMD Injector", AppResources.MainPageUnsupportedButton2, true, AppResources.MainPageUnsupportedButton1);
                if (result == 0)
                {
                    Uri uri = new Uri("https://github.com/fadilfadz01/CMD.Injector");
                    try
                    {
                        await Launcher.LaunchUriAsync(uri);
                    }
                    catch (Exception ex)
                    {
                        Helper.ThrowException(ex);
                    }
                    CoreApplication.Exit();
                    return false;
                }
                else
                {
                    CoreApplication.Exit();
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private async void Initialize()
        {
            if (!await CheckCompatibility())
            {
                return;
            };
            if (!File.Exists(@"C:\Windows\System32\Startup.bat"))
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Startup.bat", @"C:\Windows\System32\Startup.bat");
            }
            if (Helper.IsStrAGraterThanStrB(Helper.currentVersion, Helper.LocalSettingsHelper.LoadSettings("InitialLaunch", "0.0.0.0"), '.'))
            {
                Changelog.DisplayLog();
                Helper.LocalSettingsHelper.SaveSettings("InitialLaunch", Helper.currentVersion);
                Helper.LocalSettingsHelper.SaveSettings("TempInjection", true);
            }
            if ((!HomeHelper.IsCMDInjected() && !File.Exists(@"C:\Windows\System32\CMDInjector.dat")))
            {
                var isInjected = await OperationInjection();
                if (isInjected)
                {
                    if (Helper.LocalSettingsHelper.LoadSettings("FirstLaunch", true) && !File.Exists(@"C:\Windows\System32\CMDInjectorFirstLaunch.dat"))
                    {
                        var result = await Helper.DisplayMessage2(AppResources.MainPageFirstLauncDescription, Helper.SoundHelper.Sound.Alert, AppResources.MainPageFirstLaunchTitle, AppResources.MainPageFirstLauncButton2, true, AppResources.MainPageFirstLauncButton1);
                        Helper.LocalSettingsHelper.SaveSettings("FirstLaunch", false);
                        Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjectorFirstLaunch.dat");
                        if (result == 0)
                        {
                            Helper.RebootSystem();
                        }
                    }
                }
                else
                {
                    var result = await Helper.DisplayMessage2(AppResources.MainPageNDTKSvcWarningDescription, Helper.SoundHelper.Sound.Alert, "CMD Injector", AppResources.MainPageNDTKSvcWarningButton2, true, AppResources.MainPageNDTKSvcWarningButton1);
                    Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                    if (result == 0)
                    {
                        Uri uri = new Uri("https://www.google.com/search?q=How+to+interop+unlock+Windows+Phone+8%3F");
                        try
                        {
                            await Launcher.LaunchUriAsync(uri);
                        }
                        catch (Exception ex)
                        {
                            Helper.ThrowException(ex);
                        }
                    }
                    CoreApplication.Exit();
                    return;
                }
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("FirstLaunch", false);
            }
            if (Helper.LocalSettingsHelper.LoadSettings("TempInjection", true))
            {
                OperationInjection();
                Helper.LocalSettingsHelper.SaveSettings("TempInjection", false);
            }
        }

        private async Task<bool> OperationInjection()
        {
            await Task.Run(async () =>
            {
                await FileIO.WriteTextAsync(await Helper.localFolder.CreateFileAsync("CMDInjector.dat", CreationCollisionOption.ReplaceExisting), Helper.currentBatchVersion.ToString());
                Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjectorTempSetup.dat");
                if (!File.Exists(@"C:\Windows\System32\CMDInjector.bat")) Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\NonSystemWide.bat", @"C:\Windows\System32\CMDInjector.bat");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Setup.bat", @"C:\Windows\System32\CMDInjectorSetup.bat");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Bootsh\\bootshsvc.dll", @"C:\Windows\System32\bootshsvc.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Bootsh\\bootshsvc.dll.mui", @"C:\Windows\System32\en-US\bootshsvc.dll.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Startup\\startup.bsc", @"C:\Windows\System32\Boot\startup.bsc");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\bcdedit.exe.mui", @"C:\Windows\System32\en-US\bcdedit.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\cmd.exe.mui", @"C:\Windows\System32\en-US\cmd.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\reg.exe.mui", @"C:\Windows\System32\en-US\reg.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\bcdedit.exe", @"C:\Windows\System32\bcdedit.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\CaptureScreenApp.exe", @"C:\Windows\System32\CaptureScreenApp.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\cmd.exe", @"C:\Windows\System32\cmd.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\find.exe", @"C:\Windows\System32\find.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\k32.dll", @"C:\Windows\System32\k32.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\pacman_ierror.dll", @"C:\Windows\System32\pacman_ierror.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\pacmanerr.dll", @"C:\Windows\System32\pacmanerr.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\PowerTool.exe", @"C:\Windows\System32\PowerTool.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\reg.exe", @"C:\Windows\System32\reg.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ScreenSnapper.exe", @"C:\Windows\System32\ScreenSnapper.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\shutdown.exe", @"C:\Windows\System32\shutdown.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sleep.exe", @"C:\Windows\System32\sleep.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\telnetd.exe", @"C:\Windows\System32\telnetd.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TestDeploymentInfo.dll", @"C:\Windows\System32\TestDeploymentInfo.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\th.dll", @"C:\Windows\System32\th.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TH.exe", @"C:\Windows\System32\TH.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\tlist.exe", @"C:\Windows\System32\tlist.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\VolumeDown.exe", @"C:\Windows\System32\VolumeDown.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\VolumeUp.exe", @"C:\Windows\System32\VolumeUp.exe");
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
            return HomeHelper.IsCMDInjected();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (buttonOnHold)
            {
                buttonOnHold = false;
                return;
            }
            var button = sender as Button;
            var stackPanel = button.Content as StackPanel;
            var textBlock = stackPanel.Children[1] as TextBlock;
            var image = stackPanel.Children[0] as Image;
            var bitmapImage = image.Source as BitmapImage;
            if (textBlock.Text == AppResources.MainPageHomeButtonText)
            {
                NavigationService.Navigate(new Uri("/Home.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPageTerminalButtonText)
            {
                NavigationService.Navigate(new Uri("/Terminal.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPageStartupButtonText)
            {
                NavigationService.Navigate(new Uri("/Startup.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPagePacManButtonText)
            {
                NavigationService.Navigate(new Uri("/PacMan.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPageSnapperButtonText)
            {
                //Helper.DisplayMessage1(AppResources.MainPageMessageBoxContent);
                NavigationService.Navigate(new Uri("/Snapper.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPageBootConfigButtonText)
            {
                NavigationService.Navigate(new Uri("/BootConfig.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPageTweakBoxButtonText)
            {
                NavigationService.Navigate(new Uri("/TweakBox.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPageSettingsButtonText)
            {
                NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPageHelpButtonText)
            {
                NavigationService.Navigate(new Uri("/Help.xaml", UriKind.RelativeOrAbsolute));
            }
            else if (textBlock.Text == AppResources.MainPageAboutButtonText)
            {
                NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void Button_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                var button = sender as Button;
                var stackPanel = button.Content as StackPanel;
                var textBlock = stackPanel.Children[1] as TextBlock;
                if (textBlock.Text == "Terminal")
                {
                    try
                    {
                        CustomMessageBox argumentTerm = new CustomMessageBox
                        {
                            Caption = "Terminal",
                            RightButtonContent = "Cancel",
                            LeftButtonContent = "Pin"
                        };
                        TextBlock terminalTextblock = new TextBlock
                        {
                            Text = "Argument (Optional)"
                        };
                        PhoneTextBox terminalTextBox = new PhoneTextBox
                        {
                            AcceptsReturn = false,
                            Hint = "Echo Hello World!"
                        };
                        argumentTerm.Dismissed += (s, d) =>
                        {
                            if (d.Result != CustomMessageBoxResult.None)
                                flag = false;
                            if (d.Result == CustomMessageBoxResult.LeftButton)
                            {
                                if (terminalTextBox.Text == string.Empty || string.IsNullOrWhiteSpace(terminalTextBox.Text))
                                {
                                    if (ShellTile.ActiveTiles.All(a => a.NavigationUri.ToString() != $"/{textBlock.Text}.xaml"))
                                    {
                                        StandardTileData tileData = new StandardTileData();
                                        tileData.Title = textBlock.Text;
                                        tileData.BackgroundImage = new Uri($"/Assets/Icons/Menus/{textBlock.Text}MenuTileLogo.png", UriKind.RelativeOrAbsolute);
                                        tileData.Count = 0;
                                        ShellTile.Create(new Uri($"/{textBlock.Text}.xaml", UriKind.RelativeOrAbsolute), tileData);
                                    }
                                }
                                else if (ShellTile.ActiveTiles.All(a => a.NavigationUri.ToString() != $"/{textBlock.Text}.xaml?param={terminalTextBox.Text}"))
                                {
                                    StandardTileData tileData = new StandardTileData();
                                    tileData.Title = textBlock.Text + $" ({terminalTextBox.Text})";
                                    tileData.BackgroundImage = new Uri($"/Assets/Icons/Menus/{textBlock.Text}MenuTileLogo.png", UriKind.RelativeOrAbsolute);
                                    tileData.Count = 0;
                                    ShellTile.Create(new Uri($"/{textBlock.Text}.xaml?param={terminalTextBox.Text}", UriKind.RelativeOrAbsolute), tileData);
                                }
                            }
                        };
                        StackPanel terminalStackpanel = new StackPanel();
                        terminalStackpanel.Children.Add(terminalTextblock);
                        terminalStackpanel.Children.Add(terminalTextBox);
                        argumentTerm.Content = terminalStackpanel;
                        Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                        flag = true;
                        argumentTerm.Show();
                    }
                    catch (Exception ex)
                    {
                        //Helper.ThrowException(ex);
                    }
                }
                else if (ShellTile.ActiveTiles.All(a => a.NavigationUri.ToString() != $"/{textBlock.Text}.xaml"))
                {
                    StandardTileData tileData = new StandardTileData();
                    if (textBlock.Text == "Home" || textBlock.Text == "Settings" || textBlock.Text == "Help" || textBlock.Text == "About")
                        tileData.Title = textBlock.Text + " (CMD Injector)";
                    else
                        tileData.Title = textBlock.Text;
                    tileData.BackgroundImage = new Uri($"/Assets/Icons/Menus/{textBlock.Text}MenuTileLogo.png", UriKind.RelativeOrAbsolute);
                    tileData.Count = 0;
                    ShellTile.Create(new Uri($"/{textBlock.Text}.xaml", UriKind.RelativeOrAbsolute), tileData);
                }
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }
    }
}