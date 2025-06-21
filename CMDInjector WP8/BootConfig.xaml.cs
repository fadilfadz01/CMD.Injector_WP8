using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WUT_WP8.UWP;
using System.Threading;
using System.Threading.Tasks;
using CMDInjectorHelper;
using System.IO;
using System.Text.RegularExpressions;
using CMDInjector_WP8.Resources;

namespace CMDInjector_WP8
{
    public partial class BootConfig : PhoneApplicationPage
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        List<string> listPickerItems = new List<string>();
        string[] Identifier = new string[100];
        bool flag = false;

        private void Connect()
        {
            tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 1000000)
            {
                i++;
            }
        }

        private async Task SendCommand(string command)
        {
            await Task.Delay(200);
            await Task.Run(() =>
            {
                tClient.Send(command);
            });
        }

        private byte[] ToBinary(string data)
        {
            data = data.Replace("-", "");
            return Enumerable.Range(0, data.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(data.Substring(x, 2), 16)).ToArray();
        }

        /*private string[] ToMultiSZ(string data)
        {
            string[] MultiSZ = new string[0];
            MultiSZ[0] = data;
            return MultiSZ;
        }*/

        public BootConfig()
        {
            InitializeComponent();
            DefaultBox.SetValue(ListPicker.ItemCountThresholdProperty, 10);
            VolUpBox.SetValue(ListPicker.ItemCountThresholdProperty, 10);
            VolDownBox.SetValue(ListPicker.ItemCountThresholdProperty, 10);
            Connect();
            FirstRun();
        }

        private void FirstRun()
        {
            if (Helper.LocalSettingsHelper.LoadSettings("BootConfigNote", true))
            {
                Helper.DisplayMessage1(AppResources.BootConfigPageMessageBox1Content, Helper.SoundHelper.Sound.Alert, AppResources.BootConfigPageMessageBox1Title);
                Helper.LocalSettingsHelper.SaveSettings("BootConfigNote", false);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await GetEntries();
        }

        private async Task GetEntries()
        {
            try
            {
                SystemTray.IsVisible = false;
                PageLoadingStack.Visibility = Visibility.Visible;
                flag = false;
                DefaultBox.IsEnabled = false;
                SaveBtn.IsEnabled = false;
                VolUpBox.IsEnabled = false;
                VolDownBox.IsEnabled = false;
                DevMenuBtn.IsEnabled = false;
                listPickerItems.Clear();
                DefaultBox.Items.Clear();
                DisplayOrderList.Items.Clear();
                VolUpBox.Items.Clear();
                VolDownBox.Items.Clear();
                VolUpBox.Items.Add("None (Use for navigation)");
                VolDownBox.Items.Add("None (Use for navigation)");
                DescriptionBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ);

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\24000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) != string.Empty)
                {
                    string[] DisplayOrder = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\24000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ).Split(' ');
                    for (int i = 0; i < DisplayOrder.Length; i++)
                    {
                        DisplayOrderList.Items.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + DisplayOrder[i] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                    }
                }
                if (DisplayOrderList.Items.Count <= 1)
                {
                    RemoveBtn.IsEnabled = false;
                    MoveUpBtn.IsEnabled = false;
                }
                else
                {
                    RemoveBtn.IsEnabled = true;
                    MoveUpBtn.IsEnabled = true;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    ManTestSigningTog.IsChecked = true;
                }
                else
                {
                    ManTestSigningTog.IsChecked = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    ManNoIntegrityChecksTog.IsChecked = true;
                }
                else
                {
                    ManNoIntegrityChecksTog.IsChecked = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\25000004", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY) != string.Empty)
                {
                    string[] HexTimeout = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\25000004", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).ToCharArray().Select(c => c.ToString()).ToArray();
                    TimeoutBox.Text = Convert.ToString(Convert.ToInt32(HexTimeout[0] + HexTimeout[1], 16));
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\26000020", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    BootMenuTog.IsChecked = true;
                }
                else
                {
                    BootMenuTog.IsChecked = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    LoadTestSigningTog.IsChecked = true;
                }
                else
                {
                    LoadTestSigningTog.IsChecked = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    LoadNoIntegrityChecksTog.IsChecked = true;
                }
                else
                {
                    LoadNoIntegrityChecksTog.IsChecked = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\1600007e", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    LoadFlightSignTog.IsChecked = true;
                }
                else
                {
                    LoadFlightSignTog.IsChecked = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\250000c2", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY) != string.Empty)
                {
                    string[] BootMenuPol = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\250000c2", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).ToCharArray().Select(c => c.ToString()).ToArray();
                    BootMenuPolBox.SelectedIndex = Convert.ToInt32(BootMenuPol[1]);
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000040", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    AdvOptTog.IsChecked = true;
                }
                else
                {
                    AdvOptTog.IsChecked = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000041", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    OptEditTog.IsChecked = true;
                }
                else
                {
                    OptEditTog.IsChecked = false;
                }

                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    File.Delete($"{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    File.Delete($"{Helper.localFolder.Path}\\BootConfigObjects.txt");
                    await SendCommand($"for /f \"delims=\\ tokens=4\" %a in ('reg query hklm\\bcd00000001\\objects') do echo %a >>{Helper.localFolder.Path}\\BootConfigObjects.txt&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    while (File.Exists($"{Helper.localFolder.Path}\\BootConfigEnd.txt") == false)
                    {
                        await Task.Delay(200);
                    }
                    //string[] Objects = File.ReadAllLines($"{Helper.localFolder.Path}\\BootConfigObjects.txt");
                    await Task.Delay(1500);
                    string[] Objects = await GetObjects();
                    /*string toDisplay = string.Join(Environment.NewLine, Objects);
                    _ = new MessageDialog(toDisplay).ShowAsync();*/
                    var foundDevMenu = false;
                    int j = 0;
                    for (int i = 0; i < Objects.Length; i++)
                    {
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == string.Empty || Regex.Replace(Objects[i], @"\s+", "") == "{311b88b5-9b30-491d-bad9-167ca3e2d417}" || Regex.Replace(Objects[i], @"\s+", "") == "{01de5a27-8705-40db-bad6-96fa5187d4a6}" || Regex.Replace(Objects[i], @"\s+", "") == "{0ce4991b-e6b3-4b16-b23c-5e0d9250e5d9}" || Regex.Replace(Objects[i], @"\s+", "") == "{4636856e-540f-4170-a130-a84776f4c654}" || Regex.Replace(Objects[i], @"\s+", "") == "{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}" || Regex.Replace(Objects[i], @"\s+", "") == "{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}" || Regex.Replace(Objects[i], @"\s+", "") == "{ae5534e0-a924-466c-b836-758539a3ee3a}" || Regex.Replace(Objects[i], @"\s+", "") == "{9dea862c-5cdd-4e70-acc1-f32b344d4795}")
                        {
                            continue;
                        }
                        Identifier[j] = Regex.Replace(Objects[i], @"\s+", "");
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == "Developer Menu")
                        {
                            foundDevMenu = true;
                            DevMenuBtn.Content = AppResources.BootConfigPageButtonText2;
                            DevTestSigningTog.IsEnabled = true;
                            DevNoIntegrityChecksTog.IsEnabled = true;
                            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                            {
                                DevTestSigningTog.IsChecked = true;
                            }
                            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                            {
                                DevNoIntegrityChecksTog.IsChecked = true;
                            }
                        }
                        else
                        {
                            if (!foundDevMenu)
                            {
                                DevMenuBtn.Content = AppResources.BootConfigPageButtonText1;
                                DevTestSigningTog.IsEnabled = false;
                                DevNoIntegrityChecksTog.IsEnabled = false;
                            }
                        }
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\24000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ).Contains(Regex.Replace(Objects[i], @"\s+", "")) == false)
                        {
                            listPickerItems.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                        }
                        DefaultBox.Items.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                        VolUpBox.Items.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                        VolDownBox.Items.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\23000003", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == Regex.Replace(Objects[i], @"\s+", ""))
                        {
                            DefaultBox.SelectedIndex = j;
                        }
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == Regex.Replace(Objects[i], @"\s+", ""))
                        {
                            VolUpBox.SelectedIndex = j + 1;
                        }
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000002", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == Regex.Replace(Objects[i], @"\s+", ""))
                        {
                            VolDownBox.SelectedIndex = j + 1;
                        }
                        j++;
                    }
                    if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == string.Empty)
                    {
                        VolUpBox.SelectedIndex = 0;
                    }
                    if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000002", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == string.Empty)
                    {
                        VolDownBox.SelectedIndex = 0;
                    }
                    DefaultBox.IsEnabled = true;
                    if (DisplayOrderList.Items.Count > 0) SaveBtn.IsEnabled = true;
                    VolUpBox.IsEnabled = true;
                    VolDownBox.IsEnabled = true;
                    DevMenuBtn.IsEnabled = true;
                }
                DefaultBox.MinWidth = 200;
                VolUpBox.MinWidth = 200;
                VolDownBox.MinWidth = 200;
                PageLoadingStack.Visibility = Visibility.Collapsed;
                SystemTray.IsVisible = true;
                flag = true;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Index was outside the bounds of the array." || ex.Message == "Arg_IndexOutOfRangeException")
                {
                    await GetEntries();
                }
                else if (ex.Message == "Object reference not set to an instance of an object." || ex.Message == "Arg_NullReferenceException")
                {
                    return;
                }
                else
                {

                }
            }
            Helper.LocalSettingsHelper.SaveSettings("UnlockHidden", false);
        }

        private async Task<string[]> GetObjects()
        {
            using (var reader = File.OpenText($"{Helper.localFolder.Path}\\BootConfigObjects.txt"))
            {
                var Objects = await reader.ReadToEndAsync();
                return Objects.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            }
        }

        private void DefaultBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag && DefaultBox.SelectedItem != null)
            {
                //DefaultBox.IsEnabled = false;
                //statusProgress();
                //_ = sendCommand("bcdedit /set {bootmgr} default \"" + Identifier[DefaultBox.SelectedIndex] + $"\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\23000003", "Element", Helper.RegistryHelper.RegistryType.REG_SZ, Identifier[DefaultBox.SelectedIndex]);
                if (Identifier[DefaultBox.SelectedIndex] != "{7619dcc9-fafe-11d9-b411-000476eba25f}")
                {
                    Helper.DisplayMessage1(AppResources.BootConfigPageMessageBox2Content, Helper.SoundHelper.Sound.Alert, AppResources.BootConfigPageMessageBox2Title);
                }
                //DefaultBox.IsEnabled = true;
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ListPicker listPicker = new ListPicker { ItemsSource = listPickerItems };
                StackPanel stackPanel1 = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                StackPanel stackPanel2 = new StackPanel();
                CustomMessageBox customMessageBox = new CustomMessageBox { Title = "Add", Content = stackPanel2 };
                Button btn1 = new Button { Content = "Add", Width = 150 };
                if (listPickerItems.Count <= 0) btn1.IsEnabled = false;
                btn1.Click += (s, r) =>
                {
                    var clickedItem = listPicker.SelectedItem.ToString();
                    DisplayOrderList.Items.Add(clickedItem);
                    listPickerItems.Remove(clickedItem);
                    if (DisplayOrderList.Items.Count > 1)
                    {
                        RemoveBtn.IsEnabled = true;
                        MoveUpBtn.IsEnabled = true;
                    }
                    SaveBtn.IsEnabled = true;
                    customMessageBox.Dismiss();
                };
                Button btn2 = new Button { Content = "Cancel", Width = 150 };
                btn2.Click += (s, r) => { customMessageBox.Dismiss(); };
                listPicker.SetValue(ListPicker.ItemCountThresholdProperty, 10);
                stackPanel1.Children.Add(btn1);
                stackPanel1.Children.Add(btn2);
                stackPanel2.Children.Add(listPicker);
                stackPanel2.Children.Add(stackPanel1);
                customMessageBox.Show();
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayOrderList.SelectedIndex == -1)
            {
                return;
            }
            var item = DisplayOrderList.SelectedItem.ToString();
            listPickerItems.Add(item);
            DisplayOrderList.Items.Remove(DisplayOrderList.SelectedItem);
            if (DisplayOrderList.Items.Count <= 1)
            {
                RemoveBtn.IsEnabled = false;
                MoveUpBtn.IsEnabled = false;
            }
        }

        private void MoveUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayOrderList.SelectedIndex == 0 || DisplayOrderList.SelectedIndex == -1)
            {
                return;
            }
            int newIndex = DisplayOrderList.SelectedIndex - 1;
            object selectedIndex = DisplayOrderList.SelectedItem;
            DisplayOrderList.Items.Remove(selectedIndex);
            DisplayOrderList.Items.Insert(newIndex, selectedIndex);
            DisplayOrderList.SelectedIndex = newIndex;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            /*SaveBtn.IsEnabled = false;
            statusProgress();*/
            string orderList = string.Empty;
            for (int i = 0; i < DisplayOrderList.Items.Count; i++)
            {
                for (int j = 0; j < DefaultBox.Items.Count; j++)
                {
                    if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[j] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == DisplayOrderList.Items[i].ToString())
                    {
                        orderList += /*"\"" +*/ Identifier[j] + " " /*+ "\" "*/;
                        break;
                    }
                }
            }
            //await sendCommand("bcdedit /set {bootmgr} displayorder " + orderList + $"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\24000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, orderList);
            /*SaveBtn.IsEnabled = true;
            InputPane.GetForCurrentView().TryHide();*/
        }

        private void ManTestSigningTog_Checked(object sender, RoutedEventArgs e)
        {
            if (ManTestSigningTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
        }

        private void ManNoIntegrityChecksTog_Checked(object sender, RoutedEventArgs e)
        {
            if (ManNoIntegrityChecksTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
        }

        private void TimeoutBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TimeoutBox.Text == string.Empty || TimeoutBox.Text.Contains("."))
            {
                TimeoutBtn.IsEnabled = false;
            }
            else
            {
                TimeoutBtn.IsEnabled = true;
            }
        }

        private void TimeoutBtn_Click(object sender, RoutedEventArgs e)
        {
            Helper.RegistryHelper.SetRegBinaryValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\25000004", "Element", Convert.ToInt32(TimeoutBox.Text));
        }

        private void BootMenuTog_Checked(object sender, RoutedEventArgs e)
        {
            if (BootMenuTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\26000020", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\26000020", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
        }

        private async void VolUpBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag && VolUpBox.SelectedItem != null)
            {
                SystemTray.IsVisible = false;
                PageLoadingStack.Visibility = Visibility.Visible;
                VolUpBox.IsEnabled = false;
                if (VolUpBox.SelectedIndex != 0)
                {
                    if (VolDownBox.SelectedIndex != 0)
                    {
                        await SendCommand($"bcdedit /set {{bootmgr}} customactions \"0x1000048000001\" \"0x54000001\" \"0x1000050000001\" \"0x54000002\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    else
                    {
                        await SendCommand($"bcdedit /set {{bootmgr}} customactions \"0x1000048000001\" \"0x54000001\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, Regex.Replace(Identifier[VolUpBox.SelectedIndex - 1], @"\s+", ""));
                }
                else
                {
                    if (VolDownBox.SelectedIndex != 0)
                    {
                        await SendCommand($"reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\54000001\" /f & bcdedit /set {{bootmgr}} customactions  \"0x1000050000001\" \"0x54000002\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    else
                    {
                        await SendCommand($"reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\54000001\" /f & reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\27000030\" /f&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                }
                VolUpBox.IsEnabled = true;
                SystemTray.IsVisible = true;
                PageLoadingStack.Visibility = Visibility.Collapsed;
            }
        }

        private async void VolDownBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag && VolUpBox.SelectedItem != null)
            {
                SystemTray.IsVisible = false;
                PageLoadingStack.Visibility = Visibility.Visible;
                VolDownBox.IsEnabled = false;
                if (VolDownBox.SelectedIndex != 0)
                {
                    if (VolUpBox.SelectedIndex != 0)
                    {
                        await SendCommand($"bcdedit /set {{bootmgr}} customactions \"0x1000048000001\" \"0x54000001\" \"0x1000050000001\" \"0x54000002\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    else
                    {
                        await SendCommand($"bcdedit /set {{bootmgr}} customactions \"0x1000050000001\" \"0x54000002\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000002", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, Regex.Replace(Identifier[VolDownBox.SelectedIndex - 1], @"\s+", ""));
                }
                else
                {
                    if (VolUpBox.SelectedIndex != 0)
                    {
                        await SendCommand($"reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\54000002\" /f & bcdedit /set {{bootmgr}} customactions  \"0x1000048000001\" \"0x54000001\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    else
                    {
                        await SendCommand($"reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\54000002\" /f & reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\27000030\" /f&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                }
                VolDownBox.IsEnabled = true;
                SystemTray.IsVisible = true;
                PageLoadingStack.Visibility = Visibility.Collapsed;
            }
        }

        private void DescriptionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DescriptionBox.Text == string.Empty || string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                DescriptionBtn.IsEnabled = false;
            }
            else
            {
                DescriptionBtn.IsEnabled = true;
            }
        }

        private async void DescriptionBtn_Click(object sender, RoutedEventArgs e)
        {
            DescriptionBtn.IsEnabled = false;
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ, DescriptionBox.Text);
            await GetEntries();
            DescriptionBtn.IsEnabled = true;
        }

        private void LoadTestSigningTog_Checked(object sender, RoutedEventArgs e)
        {
            if (LoadTestSigningTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
        }

        private void LoadNoIntegrityChecksTog_Checked(object sender, RoutedEventArgs e)
        {
            if (LoadNoIntegrityChecksTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
        }

        private void LoadFlightSignTog_Checked(object sender, RoutedEventArgs e)
        {
            if (LoadFlightSignTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\1600007e", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Certificates\\SbcpFlightToken.p7b", "C:\\EFIESP\\efi\\Microsoft\\boot\\policies\\SbcpFlightToken.p7b");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\1600007e", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
        }

        private void AdvOptTog_Checked(object sender, RoutedEventArgs e)
        {
            if (AdvOptTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000040", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
                if (flag)
                {
                    Helper.DisplayMessage1(AppResources.BootConfigPageMessageBox3Content, Helper.SoundHelper.Sound.Alert, AppResources.BootConfigPageMessageBox3Title);
                }
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000040", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
        }

        private void OptEditTog_Checked(object sender, RoutedEventArgs e)
        {
            if (OptEditTog.IsChecked == true)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000041", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
            }
            else
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000041", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
            }
        }

        private void BootMenuPolBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag) Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\250000c2", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, $"0{BootMenuPolBox.SelectedIndex}");
        }

        private async void DevMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            DevMenuBtn.IsEnabled = false;
            File.Delete($"{Helper.localFolder.Path}\\BootConfigEnd.txt");
            if ((DevMenuBtn.Content as string) == "Install")
            {
                await SendCommand("bcdedit /create {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} /d \"Developer Menu\" /application \"bootapp\"" +
                "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} path \"\\windows\\system32\\BOOT\\developermenu.efi\"" +
                "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} device \"partition=%SystemDrive%\\Efiesp\"" +
                "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} inherit \"{bootloadersettings}\"" +
                //"&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} isolatedcontext \"yes\"" +
                "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} nointegritychecks \"yes\"" +
                $"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\developermenu.efi", "C:\\EFIESP\\Windows\\System32\\Boot\\developermenu.efi");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\ui\\boot.ums.connected.bmpx", "C:\\EFIESP\\Windows\\System32\\Boot\\ui\\boot.ums.connected.bmpx");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\ui\\boot.ums.disconnected.bmpx", "C:\\EFIESP\\Windows\\System32\\Boot\\ui\\boot.ums.disconnected.bmpx");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\ui\\boot.ums.waiting.bmpx", "C:\\EFIESP\\Windows\\System32\\Boot\\ui\\boot.ums.waiting.bmpx");
            }
            else
            {
                for (int i = 0; i < DefaultBox.Items.Count; i++)
                {
                    if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == "Developer Menu")
                    {
                        await SendCommand("bcdedit /delete \"" + Identifier[i] + $"\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                        break;
                    }
                }
            }
            while (File.Exists($"{Helper.localFolder.Path}\\BootConfigEnd.txt") == false)
            {
                await Task.Delay(200);
            }
            await GetEntries();
        }

        private void DevTestSigningTog_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < DefaultBox.Items.Count; i++)
            {
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == "Developer Menu")
                {
                    if (DevTestSigningTog.IsChecked == true)
                    {
                        Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");

                    }
                    else
                    {
                        Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
                    }
                    break;
                }
            }
        }

        private void DevNoIntegrityChecksTog_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < DefaultBox.Items.Count; i++)
            {
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == "Developer Menu")
                {
                    if (DevNoIntegrityChecksTog.IsChecked == true)
                    {
                        Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
                    }
                    else
                    {
                        Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "00");
                    }
                    break;
                }
            }
        }
    }
}