using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CMDInjectorHelper;
using WUT_WP8.UWP;
using System.Threading;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Activation;
using System.Threading.Tasks;
using Windows.Storage;
using System.Globalization;
using CMDInjector_WP8.Resources;

namespace CMDInjector_WP8
{
    public partial class Settings : PhoneApplicationPage
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient;
        bool flag = false;

        private void Connect()
        {
            tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
            tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 1000000)
            {
                i++;
            }
        }

        public Settings()
        {
            InitializeComponent();
            ConsoleFontSizeBox.SetValue(ListPicker.ItemCountThresholdProperty, 10);
            Connect();
            Initialize();
        }

        private async void Initialize()
        {
            if (Helper.LocalSettingsHelper.LoadSettings("AppCurrentLang", CultureInfo.CurrentUICulture.Name) == CultureInfo.CurrentCulture.Name)
            {
                AppLangCombo.SelectedIndex = 0;
            }
            else if (Helper.LocalSettingsHelper.LoadSettings("AppCurrentLang", CultureInfo.CurrentUICulture.Name) == "ru-RU")
            {
                AppLangCombo.SelectedIndex = 1;
            }

            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\DefaultApplications", ".xap", Helper.RegistryHelper.RegistryType.REG_SZ) == "CMDInjector_kqyng60eng17c")
            {
                DefaultTog.IsChecked = true;
            }
            else
            {
                DefaultTog.IsChecked = false;
            }

            CommandsWrapToggle.IsChecked = Helper.LocalSettingsHelper.LoadSettings("CommandsTextWrap", false);
            ConsoleFontSizeBox.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("ConFontSizeSet", 3);
            ArgConfirmTog.IsChecked = Helper.LocalSettingsHelper.LoadSettings("TerminalRunArg", true);

            StorageFolder sdCard = (await Helper.externalFolder.GetFoldersAsync()).FirstOrDefault();
            if (sdCard == null)
            {
                StorageTog.IsEnabled = false;
                StorageTog.IsChecked = false;
                Helper.LocalSettingsHelper.SaveSettings("StorageSet", false);
            }
            else
            {
                StorageTog.IsChecked = Helper.LocalSettingsHelper.LoadSettings("StorageSet", false);
            }
            if (!Helper.LocalSettingsHelper.LoadSettings("PMLogPath", false))
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PMLogPath", KnownFolders.DocumentsLibrary);
                Helper.LocalSettingsHelper.SaveSettings("PMLogPath", true);
            }
            if ((await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMLogPath")).Path == string.Empty)
            {
                LogPathBox.Text = "C:\\Data\\Users\\Public\\Documents\\PacMan_Installer.pmlog";
            }
            else
            {
                LogPathBox.Text = $"{(await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMLogPath")).Path}\\PacMan_Installer.pmlog";
            }

            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000004")
            {
                BootshToggle.IsChecked = false;
            }
            else
            {
                BootshToggle.IsChecked = true;
            }

            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
            {
                UMCIToggle.IsChecked = true;
            }
            else
            {
                UMCIToggle.IsChecked = false;
            }

            flag = true;
        }

        private void AppLangCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag)
            {
                if (AppLangCombo.SelectedIndex == 0)
                {
                    Helper.LocalSettingsHelper.SaveSettings("AppCurrentLang", CultureInfo.CurrentCulture.Name);
                }
                else if (AppLangCombo.SelectedIndex == 1)
                {
                    Helper.LocalSettingsHelper.SaveSettings("AppCurrentLang", "ru-RU");
                }
                Helper.DisplayMessage1(AppResources.SettingsPageMessageBox1Content, Helper.SoundHelper.Sound.Alert);
            }
        }

        private void ConsoleFontSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag)
                Helper.LocalSettingsHelper.SaveSettings("ConFontSizeSet", ConsoleFontSizeBox.SelectedIndex);
        }

        private void ArgConfirmTog_Checked(object sender, RoutedEventArgs e)
        {
            if (ArgConfirmTog.IsChecked == true)
            {
                Helper.LocalSettingsHelper.SaveSettings("TerminalRunArg", true);
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("TerminalRunArg", false);
            }
        }

        private void CommandsWrapToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (flag)
            {
                if (CommandsWrapToggle.IsChecked == true)
                {
                    Helper.LocalSettingsHelper.SaveSettings("CommandsTextWrap", true);
                }
                else
                {
                    Helper.LocalSettingsHelper.SaveSettings("CommandsTextWrap", false);
                }
            }
        }

        private async void StartupRstBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await Helper.DisplayMessage2(AppResources.SettingsPageMessageBox2Content, Helper.SoundHelper.Sound.Alert, "", AppResources.SettingsPageMessageBox2Button2Text, true, AppResources.SettingsPageMessageBox2Button1Text);
            if (result == 0)
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Startup.bat", @"C:\Windows\System32\Startup.bat");
            }
        }

        private void DefaultTog_Checked(object sender, RoutedEventArgs e)
        {
            if (DefaultTog.IsChecked == true)
            {
                try
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\DefaultApplications", ".xap", Helper.RegistryHelper.RegistryType.REG_SZ, "CMDInjector_kqyng60eng17c");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\DefaultApplications", ".appx", Helper.RegistryHelper.RegistryType.REG_SZ, "CMDInjector_kqyng60eng17c");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\DefaultApplications", ".appxbundle", Helper.RegistryHelper.RegistryType.REG_SZ, "CMDInjector_kqyng60eng17c");
                }
                catch (Exception ex) { Helper.ThrowException(ex); }
            }
            else
            {
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    tClient.Send("reg delete HKLM\\Software\\Microsoft\\DefaultApplications /v .xap /f" +
                        "&reg delete HKLM\\Software\\Microsoft\\DefaultApplications /v .appx /f" +
                        "&reg delete HKLM\\Software\\Microsoft\\DefaultApplications /v .appxbundle /f");
                }
                else
                {
                    Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            if (flag)
            {
                DefInstIndicator.Visibility = Visibility.Visible;
            }
        }

        private void StorageTog_Checked(object sender, RoutedEventArgs e)
        {
            if (StorageTog.IsChecked == true)
            {
                Helper.LocalSettingsHelper.SaveSettings("StorageSet", true);
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("StorageSet", false);
            }
        }

        private async void LogPathBtn_Click(object sender, RoutedEventArgs e)
        {
            bool completed = false;
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            CoreApplication.GetCurrentView().Activated += (s, a) =>
            {
                var continuationEventArgs = a as FolderPickerContinuationEventArgs;
                if (continuationEventArgs != null)
                {
                    var folder = continuationEventArgs.Folder;
                    if (folder != null)
                    {
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace("PMLogPath", folder);
                    }
                }
                completed = true;
            };
            folderPicker.PickFolderAndContinue();
            while (!completed)
            {
                await Task.Delay(200);
            }
            Connect();
            LogPathBox.Text = $"{(await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMLogPath")).Path}\\PacMan_Installer.pmlog";
        }

        private void BootshToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (BootshToggle.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000002");
            }
            else
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000004");
            }
            if (flag)
            {
                BootshIndicator.Visibility = Visibility.Visible;
            }
        }

        private void UMCIToggle_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UMCIToggle.IsChecked == true)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                }
                else
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
            if (flag == true)
            {
                UMCIModeIndicator.Visibility = Visibility.Visible;
            }
        }
    }
}