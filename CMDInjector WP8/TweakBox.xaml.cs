using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
using WUT_WP8.UWP;
using CMDInjectorHelper;
using Windows.ApplicationModel.Core;
using DevProgram;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Reflection;
using CMDInjector_WP8.Resources;

namespace CMDInjector_WP8
{
    public partial class TweakBox : PhoneApplicationPage
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient;
        bool buttonOnHold = false;
        bool flag = false;

        public TweakBox()
        {
            InitializeComponent();
            FontFileBox.SetValue(ListPicker.ItemCountThresholdProperty, 10);
            ColorPickCombo.SetValue(ListPicker.ItemCountThresholdProperty, 50);
        }

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                Connect();
                Initialize();
                if (NavigationContext.QueryString["param"] == "Restart")
                {
                    Helper.RebootSystem();
                    Helper.DisplayMessage1(AppResources.TweakBoxPageSystemMessageBox1Content, Helper.SoundHelper.Sound.Alert);
                }
                else if (NavigationContext.QueryString["param"] == "Lockscreen" || NavigationContext.QueryString["param"] == "Shutdown" || NavigationContext.QueryString["param"] == "VolumeUp" || NavigationContext.QueryString["param"] == "VolumeDown")
                {
                    if (tClient.IsConnected && HomeHelper.IsConnected())
                    {
                        switch (NavigationContext.QueryString["param"])
                        {
                            case "Lockscreen":
                                tClient.Send("powertool -screenoff");
                                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Lock);
                                CoreApplication.Exit();
                                break;
                            case "Shutdown":
                                tClient.Send("shutdown /s /t 0");
                                Helper.DisplayMessage1(AppResources.TweakBoxPageSystemMessageBox2Content, Helper.SoundHelper.Sound.Alert);
                                break;
                            case "VolumeUp":
                                tClient.Send("VolumeUp.exe");
                                break;
                            case "VolumeDown":
                                tClient.Send("VolumeDown.exe");
                                break;
                        }
                    }
                    else
                    {
                        Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                    }
                }
            }
            catch (Exception ex)
            {
                //Helper.ThrowException(ex);
            }
        }

        private async void Initialize()
        {
            try
            {
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Display\\ColorAndLight", "UserSettingNoBrightnessSettings", Helper.RegistryHelper.RegistryType.REG_DWORD) != string.Empty)
                    BrightTog.IsChecked = true;

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Platform\\DeviceTargetingInfo", "PhoneManufacturerModelName", Helper.RegistryHelper.RegistryType.REG_DWORD).StartsWith("RM-RM-938_", StringComparison.OrdinalIgnoreCase))
                    PhabletTog.IsChecked = true;

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_WVGA.bin", StringComparison.OrdinalIgnoreCase) >= 0)
                    FontFileBox.SelectedIndex = 1;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_720.bin", StringComparison.OrdinalIgnoreCase) >= 0)
                    FontFileBox.SelectedIndex = 2;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_720_hi.bin", StringComparison.OrdinalIgnoreCase) >= 0)
                    FontFileBox.SelectedIndex = 3;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_WXGA.bin", StringComparison.OrdinalIgnoreCase) >= 0)
                    FontFileBox.SelectedIndex = 4;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_FHD.bin", StringComparison.OrdinalIgnoreCase) >= 0)
                    FontFileBox.SelectedIndex = 5;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_WQHD.bin", StringComparison.OrdinalIgnoreCase) >= 0)
                    FontFileBox.SelectedIndex = 6;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_WQHD_hi.bin", StringComparison.OrdinalIgnoreCase) >= 0)
                    FontFileBox.SelectedIndex = 7;
                var ClockAndIndicatorsCustomColor = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD);
                if (ClockAndIndicatorsCustomColor != string.Empty && ClockAndIndicatorsCustomColor != "00000000")
                {
                    FontColorTog.IsChecked = true;
                    FontColorStack.Visibility = Visibility.Visible;
                    if (ClockAndIndicatorsCustomColor == "00ff0000")
                        RedRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == "0000ff00")
                        GreenRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == "000000ff")
                        BlueRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == "0000ffff")
                        CyanRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == "00ff00ff")
                        MagentaRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == "00ffff00")
                        YellowRadio.IsChecked = true;
                }
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "MoveClock", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
                    MoveClockTog.IsChecked = true;

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\16000069", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01") && Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\1600007a", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                    BootAnimTog.IsChecked = false;
                else
                    BootAnimTog.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ) != string.Empty)
                    BootImageTog.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ) != string.Empty)
                    ShutdownImageTog.IsChecked = true;

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "SoftwareModeEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
                    SoftNavTog.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsDoubleTapOffEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
                    DoubleTapTog.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsAutoHideEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
                    AutoHideTog.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsSwipeUpToHideEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
                    SwipeUpTog.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsUserManaged", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
                    UserManagedTog.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsBurnInProtectionEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
                    BurninProtTog.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIdleTimerTimeout", Helper.RegistryHelper.RegistryType.REG_DWORD) != string.Empty)
                    BurninTimeoutBox.Text = Convert.ToInt32(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIdleTimerTimeout", Helper.RegistryHelper.RegistryType.REG_DWORD), 16).ToString();
                var BurnInProtectionBlackReplacementColor = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionBlackReplacementColor", Helper.RegistryHelper.RegistryType.REG_DWORD);
                foreach (var color in typeof(Colors).GetRuntimeProperties())
                {
                    if (color.Name != "AliceBlue" && color.Name != "AntiqueWhite" && color.Name != "Azure" && color.Name != "Beige" && color.Name != "Bisque" && color.Name != "Black" && color.Name != "BlanchedAlmond" && color.Name != "Cornsilk" && color.Name != "FloralWhite" && color.Name != "Gainsboro" && color.Name != "GhostWhite" && color.Name != "Honeydew" && color.Name != "Ivory" && color.Name != "Lavender" && color.Name != "LavenderBlush" && color.Name != "LemonChiffon"
                    && color.Name != "LightCyan" && color.Name != "LightGoldenrodYellow" && color.Name != "LightGray" && color.Name != "LightYellow" && color.Name != "Linen" && color.Name != "MintCream" && color.Name != "MistyRose" && color.Name != "Moccasin" && color.Name != "OldLace" && color.Name != "PapayaWhip" && color.Name != "SeaShell" && color.Name != "Snow" && color.Name != "Transparent" && color.Name != "White" && color.Name != "WhiteSmoke")
                    {
                        var selectColor = new Rectangle { Width = 20, Height = 20, Margin = new Thickness(0, 0, 10, 0), Fill = new SolidColorBrush((Color)color.GetValue(null)) };
                        var colorText = new TextBlock { Text = color.Name, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush((Color)color.GetValue(null)) };
                        var colorStack = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
                        colorStack.Children.Add(selectColor);
                        colorStack.Children.Add(colorText);
                        var cbi = new ComboBoxItem { Content = colorStack };
                        ColorPickCombo.Items.Add(cbi);
                        SolidColorBrush solidColor = (SolidColorBrush)selectColor.Fill;
                        if (BurnInProtectionBlackReplacementColor != string.Empty && Convert.ToInt32(solidColor.Color.ToString().Remove(0, 3), 16) == Convert.ToInt32(BurnInProtectionBlackReplacementColor, 16))
                        {
                            ColorPickCombo.SelectedItem = cbi;
                        }
                    }
                }
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIconsOpacity", Helper.RegistryHelper.RegistryType.REG_DWORD) != string.Empty)
                    OpacitySlide.Value = Convert.ToInt32(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIconsOpacity", Helper.RegistryHelper.RegistryType.REG_DWORD), 16);

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Camera\\Barc", "DNGDisabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000000")
                    DngTog.IsChecked = true;

                VirtualMemBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\CurrentControlSet\\Control\\Session Manager\\Memory Management", "PagingFiles", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ);

                await Task.Delay(200);
                flag = true;
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private async void ShutBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                var result = await Helper.DisplayMessage2(AppResources.TweakBoxPageSystemMessageBox3Content, Helper.SoundHelper.Sound.Alert, "", AppResources.TweakBoxPageSystemMessageBox3Button2content, true, AppResources.TweakBoxPageSystemMessageBox3Button1content);
                if (result == 0)
                {
                    if (tClient.IsConnected && HomeHelper.IsConnected())
                    {
                        tClient.Send("shutdown /s /t 0");
                    }
                    else
                    {
                        Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                    }
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                var result = await Helper.DisplayMessage2(AppResources.TweakBoxPageSystemMessageBox4Content, Helper.SoundHelper.Sound.Alert, "", AppResources.TweakBoxPageSystemMessageBox4Button2content, true, AppResources.TweakBoxPageSystemMessageBox4Button1content);
                if (result == 0)
                {
                    Helper.RebootSystem();
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void LockscreenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    tClient.Send("powertool -screenoff");
                    Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Lock);
                }
                else
                {
                    Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void FFULoaderBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShutBtn_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (ShellTile.ActiveTiles.All(a => a.NavigationUri.ToString() != $"/TweakBox.xaml?param=Shutdown"))
                {
                    StandardTileData tileData = new StandardTileData();
                    tileData.Title = "Shutdown";
                    tileData.BackgroundImage = new Uri($"/Assets/Icons/PowerOptions/ShutdownPowerOptionTileLogo.png", UriKind.RelativeOrAbsolute);
                    tileData.Count = 0;
                    ShellTile.Create(new Uri($"/TweakBox.xaml?param=Shutdown", UriKind.RelativeOrAbsolute), tileData);
                    TipText.Visibility = Visibility.Collapsed;
                    Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                }
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private void RestartBtn_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (ShellTile.ActiveTiles.All(a => a.NavigationUri.ToString() != $"/TweakBox.xaml?param=Restart"))
                {
                    StandardTileData tileData = new StandardTileData();
                    tileData.Title = "Restart";
                    tileData.BackgroundImage = new Uri($"/Assets/Icons/PowerOptions/RestartPowerOptionTileLogo.png", UriKind.RelativeOrAbsolute);
                    tileData.Count = 0;
                    ShellTile.Create(new Uri($"/TweakBox.xaml?param=Restart", UriKind.RelativeOrAbsolute), tileData);
                    TipText.Visibility = Visibility.Collapsed;
                    Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                }
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private void LockscreenBtn_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (ShellTile.ActiveTiles.All(a => a.NavigationUri.ToString() != $"/TweakBox.xaml?param=Lockscreen"))
                {
                    StandardTileData tileData = new StandardTileData();
                    tileData.Title = "Lockscreen";
                    tileData.BackgroundImage = new Uri($"/Assets/Icons/PowerOptions/LockscreenPowerOptionTileLogo.png", UriKind.RelativeOrAbsolute);
                    tileData.Count = 0;
                    ShellTile.Create(new Uri($"/TweakBox.xaml?param=Lockscreen", UriKind.RelativeOrAbsolute), tileData);
                    TipText.Visibility = Visibility.Collapsed;
                    Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                }
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private void FFULoaderBtn_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private void VolumeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (buttonOnHold)
            {
                buttonOnHold = false;
                return;
            }
            if (tClient.IsConnected && HomeHelper.IsConnected())
            {
                var button = sender as Button;
                if (button.Content.ToString() == "Volume Up")
                {
                    tClient.Send("VolumeUp.exe");
                }
                else if (button.Content.ToString() == "Volume Down")
                {
                    tClient.Send("VolumeDown.exe");
                }
            }
            else
            {
                Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
            }
        }

        private void VolumeBtn_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var button = sender as Button;
                buttonOnHold = true;
                StandardTileData tileData = new StandardTileData();
                tileData.Title = button.Content as string;
                tileData.BackgroundImage = new Uri($"/Assets/Icons/VolumeOptions/{tileData.Title.Split(' ')[1]}VolumeOptionTileLogo.png", UriKind.RelativeOrAbsolute);
                tileData.Count = 0;
                ShellTile.Create(new Uri($"/TweakBox.xaml?param=Volume{tileData.Title.Split(' ')[1]}", UriKind.RelativeOrAbsolute), tileData);
                TipText.Visibility = Visibility.Collapsed;
                Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
            }
            catch (Exception ex) { /*Helper.ThrowException(ex);*/ }
        }

        private void BrightTog_Checked(object sender, RoutedEventArgs e)
        {
            if (BrightTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Display\\ColorAndLight", "UserSettingNoBrightnessSettings", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
            }
            else
            {
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    tClient.Send("reg delete HKLM\\SOFTWARE\\OEM\\NOKIA\\Display\\ColorAndLight /v UserSettingNoBrightnessSettings /f");
                }
                else
                {
                    Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            if (flag)
            {
                BrightTogIndicator.Visibility = Visibility.Visible;
            }
        }

        private void PhabletTog_Checked(object sender, RoutedEventArgs e)
        {
            if (PhabletTog.IsChecked == true)
            {
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Platform\\DeviceTargetingInfo", "PhoneManufacturerModelNameBak", Helper.RegistryHelper.RegistryType.REG_SZ) == string.Empty)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Platform\\DeviceTargetingInfo", "PhoneManufacturerModelNameBak", Helper.RegistryHelper.RegistryType.REG_SZ, Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Platform\\DeviceTargetingInfo", "PhoneManufacturerModelName", Helper.RegistryHelper.RegistryType.REG_SZ));
                }
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Platform\\DeviceTargetingInfo", "PhoneManufacturerModelName", Helper.RegistryHelper.RegistryType.REG_SZ, "RM-938_1000");
            }
            else
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Platform\\DeviceTargetingInfo", "PhoneManufacturerModelName", Helper.RegistryHelper.RegistryType.REG_SZ, Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Platform\\DeviceTargetingInfo", "PhoneManufacturerModelNameBak", Helper.RegistryHelper.RegistryType.REG_SZ));
            }
            if (flag)
            {
                PhabletTogIndicator.Visibility = Visibility.Visible;
            }
        }

        private void FontFileBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag)
            {
                if (FontFileBox.SelectedIndex == 1) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WVGA.bin");
                else if (FontFileBox.SelectedIndex == 2) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_720.bin");
                else if (FontFileBox.SelectedIndex == 3) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_720_hi.bin");
                else if (FontFileBox.SelectedIndex == 4) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WXGA.bin");
                else if (FontFileBox.SelectedIndex == 5) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_FHD.bin");
                else if (FontFileBox.SelectedIndex == 6) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WQHD.bin");
                else if (FontFileBox.SelectedIndex == 7) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WQHD_hi.bin");
            }
        }

        private void FontColorTog_Checked(object sender, RoutedEventArgs e)
        {
            if (flag)
            {
                if (FontColorTog.IsChecked == true)
                {
                    Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, "16711680");
                    FontColorStack.Visibility = Visibility.Visible;
                    RedRadio.IsChecked = true;
                }
                else
                {
                    Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, "0");
                    FontColorStack.Visibility = Visibility.Collapsed;
                    RedRadio.IsChecked = false;
                    GreenRadio.IsChecked = false;
                    BlueRadio.IsChecked = false;
                    CyanRadio.IsChecked = false;
                    MagentaRadio.IsChecked = false;
                    YellowRadio.IsChecked = false;
                }
            }
        }

        private void FontColor_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Content as string == "Red") Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, "16711680");
            else if (rb.Content as string == "Green") Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, "65280");
            else if (rb.Content as string == "Blue") Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, "255");
            else if (rb.Content as string == "Cyan") Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, "65535");
            else if (rb.Content as string == "Magenta") Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, "16711935");
            else if (rb.Content as string == "Yellow") Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, "16776960");
        }

        private void MoveClockTog_Checked(object sender, RoutedEventArgs e)
        {
            if (MoveClockTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "MoveClock", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "MoveClock", Helper.RegistryHelper.RegistryType.REG_DWORD, "0");
            }
        }

        private void BootAnimTog_Checked(object sender, RoutedEventArgs e)
        {
            if (BootAnimTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\16000069", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\1600007a", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\16000069", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\1600007a", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
            }
        }

        private void BootImageTog_Checked(object sender, RoutedEventArgs e)
        {
            if (BootImageTog.IsChecked == true)
            {
                BootImageStack.Visibility = Visibility.Visible;
                if (flag == true)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ, Helper.installedLocation.Path + "\\Assets\\Images\\Bootscreens\\BootupImage.png");
                }
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ).Contains(Helper.installedLocation.Path))
                {
                    BootImageBox.Text = $"CMDInjector:\\Assets\\Images\\Bootscreens\\BootupImage.png";
                }
                else
                {
                    BootImageBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ);
                }
            }
            else
            {
                DevProgramReg.DeleteValue(RegistryHive.HKLM, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride");
                BootImageStack.Visibility = Visibility.Collapsed;
            }
        }

        private void BootImageBtn_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            CoreApplication.GetCurrentView().Activated += (s, a) =>
            {
                var continuationEventArgs = a as FileOpenPickerContinuationEventArgs;
                if (continuationEventArgs != null)
                {
                    var files = continuationEventArgs.Files;
                    if (files.Count > 0)
                    {
                        BootImageBox.Text = files[0].Path;
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ, files[0].Path);
                    }
                }
            };
            picker.PickSingleFileAndContinue();
        }

        private void ShutdownImageTog_Checked(object sender, RoutedEventArgs e)
        {
            if (ShutdownImageTog.IsChecked == true)
            {
                ShutdownImageStack.Visibility = Visibility.Visible;
                if (flag == true)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ, Helper.installedLocation.Path + "\\Assets\\Images\\Bootscreens\\ShutdownImage.png");
                }
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ).Contains(Helper.installedLocation.Path))
                {
                    ShutdownImageBox.Text = $"CMDInjector:\\Assets\\Images\\Bootscreens\\ShutdownImage.png";
                }
                else
                {
                    ShutdownImageBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ);
                }
            }
            else
            {
                DevProgramReg.DeleteValue(RegistryHive.HKLM, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride");
                ShutdownImageStack.Visibility = Visibility.Collapsed;
            }
        }

        private void ShutdownImageBtn_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            CoreApplication.GetCurrentView().Activated += (s, a) =>
            {
                var continuationEventArgs = a as FileOpenPickerContinuationEventArgs;
                if (continuationEventArgs != null)
                {
                    var files = continuationEventArgs.Files;
                    if (files.Count > 0)
                    {
                        ShutdownImageBox.Text = files[0].Path;
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ, files[0].Path);
                    }
                }
            };
            picker.PickSingleFileAndContinue();
        }

        private void SoftNavTog_Checked(object sender, RoutedEventArgs e)
        {
            if (SoftNavTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "SoftwareModeEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "SoftwareModeEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "0");
            }
            if (flag == true)
            {
                SoftwareModeIndicator.Visibility = Visibility.Visible;
            }
        }

        private void DoubleTapTog_Checked(object sender, RoutedEventArgs e)
        {
            if (DoubleTapTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsDoubleTapOffEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsDoubleTapOffEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "0");
            }
            if (flag == true)
            {
                DoubleTapIndicator.Visibility = Visibility.Visible;
            }
        }

        private void AutoHideTog_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoHideTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsAutoHideEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsAutoHideEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "0");
            }
            if (flag == true)
            {
                AutoHideIndicator.Visibility = Visibility.Visible;
            }
        }

        private void SwipeUpTog_Checked(object sender, RoutedEventArgs e)
        {
            if (SwipeUpTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsSwipeUpToHideEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsSwipeUpToHideEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "0");
            }
            if (flag == true)
            {
                SwipeUpIndicator.Visibility = Visibility.Visible;
            }
        }

        private void UserManagedTog_Checked(object sender, RoutedEventArgs e)
        {
            if (SwipeUpTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsUserManaged", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsUserManaged", Helper.RegistryHelper.RegistryType.REG_DWORD, "0");
            }
            if (flag == true)
            {
                UserManagedIndicator.Visibility = Visibility.Visible;
            }
        }

        private void BurninProtTog_Checked(object sender, RoutedEventArgs e)
        {
            if (BurninProtTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsBurnInProtectionEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionMaskSwitchingInterval", Helper.RegistryHelper.RegistryType.REG_DWORD, "1");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsBurnInProtectionEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "0");
            }
            if (flag == true)
            {
                BurnInIndicator.Visibility = Visibility.Visible;
            }
        }

        private void BurninTimeoutBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BurninTimeoutBox.Text == string.Empty)
            {
                BurninTimeoutBtn.IsEnabled = false;
            }
            else
            {
                BurninTimeoutBtn.IsEnabled = true;
            }
        }

        private void BurninTimeoutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (BurninTimeoutBox.Text != string.Empty)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIdleTimerTimeout", Helper.RegistryHelper.RegistryType.REG_DWORD, BurninTimeoutBox.Text);
                BurnInTimeoutIndicator.Visibility = Visibility.Visible;
            }
        }

        private void ColorPickCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag == true && ColorPickCombo.SelectedIndex != 0)
            {
                ComboBoxItem cbi = ColorPickCombo.SelectedItem as ComboBoxItem;
                StackPanel stackPanel = cbi.Content as StackPanel;
                Brush selectedColor = (stackPanel.Children[0] as Rectangle).Fill;
                SolidColorBrush solidColor = (SolidColorBrush)selectedColor;
                string hexColor = solidColor.Color.ToString().Remove(0, 3);
                int decimalColor = Convert.ToInt32(hexColor, 16);
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionBlackReplacementColor", Helper.RegistryHelper.RegistryType.REG_DWORD, decimalColor.ToString());
                BurnInColorIndicator.Visibility = Visibility.Visible;
            }
        }

        private void OpacitySlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIconsOpacity", Helper.RegistryHelper.RegistryType.REG_DWORD, Math.Round(e.NewValue).ToString());
                if (flag == true)
                {
                    BurnInOpacityIndicator.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private void DngTog_Checked(object sender, RoutedEventArgs e)
        {
            if (DngTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Camera\\Barc", "DNGDisabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
            }
            else
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Camera\\Barc", "DNGDisabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
            }
        }

        private void VirtualMemBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VirtualMemBox.Text == string.Empty || string.IsNullOrWhiteSpace(VirtualMemBox.Text))
            {
                VirtualMemBtn.IsEnabled = false;
            }
            else
            {
                VirtualMemBtn.IsEnabled = true;
            }
        }

        private void VirtualMemBtn_Click(object sender, RoutedEventArgs e)
        {
            if (VirtualMemBox.Text != string.Empty)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\CurrentControlSet\\Control\\Session Manager\\Memory Management", "PagingFiles", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, VirtualMemBox.Text);
                VirtualMemoryIndicator.Visibility = Visibility.Visible;
            }
        }
    }
}