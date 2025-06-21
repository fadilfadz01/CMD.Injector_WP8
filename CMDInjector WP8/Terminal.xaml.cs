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
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.ViewManagement;
using Windows.Storage;
using CMDInjector_WP8.Resources;
using System.Windows.Documents;

namespace CMDInjector_WP8
{
    public partial class Terminal : PhoneApplicationPage
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        bool flag = false;

        public Terminal()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                if (HomeHelper.IsCMDInjected())
                {
                    await Connect();
                    File.Delete(TerminalHelper.EndFile);
                    await Task.Delay(200);
                    await SendCommand($"echo %cd%^> >{TerminalHelper.DirectoryFile} 2>&1&echo. >{TerminalHelper.EndFile}");
                    while (File.Exists(TerminalHelper.EndFile) == false)
                    {
                        await Task.Delay(100);
                    }
                    ConsoleBox.Text = $"Microsoft Windows [Version {Helper.build}]\r\n(c) 2013 Microsoft Corporation. All rights reserved.\r\n";
                    using (StreamReader reader = new StreamReader(TerminalHelper.DirectoryFile))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                            DirLabel.Text = Regex.Replace(line, @"\s+", "");
                    }
                    //for testing
                    //TelnetCommand.Text = "";
                }
                else
                {
                    Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                    TelnetCommandBox.IsReadOnly = true;
                    CommandSendBtn.IsEnabled = false;
                }
                flag = true;
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async Task Connect()
        {
            ConsoleBox.Text = AppResources.TerminalPageConnectionDescription1;
            tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 150)
            {
                await Task.Delay(100);
                i++;
            }
            if (tClient.IsConnected && HomeHelper.IsConnected())
            {
                ConsoleBox.Text = AppResources.TerminalPageConnectionDescription2;
            }
            /*else
            {
                //ConsoleBox.Text = HomeHelper.GetTelnetTroubleshoot();
                TelnetCommandBox.IsReadOnly = true;
                CommandSendBtn.IsEnabled = false;
            }*/
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ConsoleBox.FontSize = TerminalHelper.FontSize;
            TempBox.FontSize = TerminalHelper.FontSize;
            DirLabel.FontSize = TerminalHelper.FontSize;
            try
            {
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    if (NavigationContext.QueryString["param"] != string.Empty)
                    {
                        var cmd = NavigationContext.QueryString["param"];
                        if (Helper.LocalSettingsHelper.LoadSettings("TerminalRunArg", true))
                        {
                            if (Helper.LocalSettingsHelper.LoadSettings("TerminalRunArg", true))
                            {
                                TextBlock textBlockAsk = new TextBlock
                                {
                                    Text = "\nDo you want to run the argument?"
                                };
                                TextBlock textBlockArg = new TextBlock();
                                textBlockArg.Inlines.Add(new Run() { Text = "\n" + cmd.Substring(cmd.IndexOf(" ") + 1), FontStyle = FontStyles.Italic });
                                CheckBox checkBox = new CheckBox
                                {
                                    Content = "Don't ask again"
                                };
                                StackPanel stackPanel = new StackPanel();
                                stackPanel.Children.Add(textBlockArg);
                                stackPanel.Children.Add(textBlockAsk);
                                stackPanel.Children.Add(checkBox);
                                CustomMessageBox customMessageBox = new CustomMessageBox
                                {
                                    Caption = "Terminal",
                                    Content = stackPanel,
                                    LeftButtonContent = "Run",
                                    RightButtonContent = "Cancel"
                                };
                                customMessageBox.Dismissed += (s, d) =>
                                {
                                    if (checkBox.IsChecked == true)
                                    {
                                        Helper.LocalSettingsHelper.SaveSettings("TerminalRunArg", false);
                                    }
                                    if (d.Result != CustomMessageBoxResult.LeftButton)
                                    {
                                        return;
                                    }
                                };
                                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                                customMessageBox.Show();
                            }
                        }
                        while (flag == false)
                        {
                            await Task.Delay(100);
                        }
                        TelnetCommandBox.Text = cmd;
                            CommandRun();
                    }
                }
            }
            catch (Exception ex)
            {
                //Helper.ThrowException(ex);
            }
        }

        private async Task SendCommand(string command)
        {
            await Task.Run(() =>
            {
                tClient.Send(command);
            });
        }

        private void CommandSendBtn_Click(object sender, RoutedEventArgs e)
        {
            CommandRun();
        }

        private void TelnetCommandBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TelnetCommandBox.Text == string.Empty || string.IsNullOrWhiteSpace(TelnetCommandBox.Text))
            {
                CommandSendBtn.IsEnabled = false;
            }
            else
            {
                CommandSendBtn.IsEnabled = true;
            }
        }

        private async void CommandRun()
        {
            InputPane.GetForCurrentView().TryHide();
            if (TelnetCommandBox.Text.ToLower() == "exit")
            {
                TelnetCommandBox.Text = string.Empty;
                DirLabel.Text = string.Empty;
                TelnetCommandBox.IsHitTestVisible = false;
                CommandSendBtn.IsEnabled = false;
                ConsoleBox.Text = AppResources.TerminalPageConnectionDescription3;
                tClient.Dispose();
            }
            else if (TelnetCommandBox.Text.ToLower() == "cls")
            {
                ConsoleBox.Text = string.Empty;
                DirLabel.Text = Regex.Replace((await FileIO.ReadLinesAsync((await StorageFile.GetFileFromPathAsync(TerminalHelper.DirectoryFile)))).First(), @"\s+", "");
                TelnetCommandBox.Text = string.Empty;
            }
            else if (TelnetCommandBox.Text.ToLower() == "cmd")
            {
                ConsoleBox.Text += "\r\n" + DirLabel.Text + TelnetCommandBox.Text + $"\r\nMicrosoft Windows [Version {Helper.build}]\r\n(c) 2013 Microsoft Corporation. All rights reserved.\r\n";
                DirLabel.Text = Regex.Replace((await FileIO.ReadLinesAsync((await StorageFile.GetFileFromPathAsync(TerminalHelper.DirectoryFile)))).First(), @"\s+", "");
                TelnetCommandBox.Text = string.Empty;
            }
            else if (TelnetCommandBox.Text.ToLower() == "cmdinjector -unlock")
            {
                Helper.LocalSettingsHelper.SaveSettings("UnlockHidden", true);
                this.TelnetCommandBox.Text = string.Empty;
            }
            else
            {
                try
                {
                    ConsoleBox.Text += "\r\n" + DirLabel.Text + TelnetCommandBox.Text;
                    DirLabel.Text = string.Empty;
                    File.Delete(TerminalHelper.ResultFile);
                    await SendCommand(TelnetCommandBox.Text + $" >>{TerminalHelper.ResultFile} 2>&1&echo Executed successfully. >>{TerminalHelper.ResultFile}");
                    TelnetCommandBox.Text = string.Empty;
                    TelnetCommandBox.IsHitTestVisible = false;
                    CommandSendBtn.IsEnabled = false;
                    var temp = string.Empty;
                    await Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (File.Exists(TerminalHelper.ResultFile)) break;
                        }
                        while (true)
                        {
                            using (var fs = new FileStream(TerminalHelper.ResultFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var sr = new StreamReader(fs))
                            {
                                var output = await sr.ReadToEndAsync();
                                if (temp != output)
                                {
                                    temp = output;
                                    var result = string.Empty;
                                    if (output.Split('\n').Length >= 2)
                                    {
                                        result = output.Split('\n')[output.Split('\n').Length - 2];
                                    }
                                    if (result.Contains("Executed successfully."))
                                    {
                                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                        {
                                            try
                                            {
                                                TempBox.Text = output.Remove(output.TrimEnd().LastIndexOf(Environment.NewLine));
                                            }
                                            catch (Exception ex)
                                            {
                                                //Helper.ThrowException(ex);
                                            }
                                        });
                                        break;
                                    }
                                    else
                                    {
                                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                        {
                                            TempBox.Text = output;
                                        });
                                    }
                                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        TempBox.Visibility = Visibility.Visible;
                                    });
                                }
                            }
                        }
                    });
                    ConsoleBox.Text += "\r\n" + TempBox.Text;
                    if (TempBox.Text != string.Empty) ConsoleBox.Text += "\r\n";
                    TempBox.Visibility = Visibility.Collapsed;
                    TempBox.Text = string.Empty;
                    File.Delete(TerminalHelper.EndFile);
                    await SendCommand($"echo %cd%^> >{TerminalHelper.DirectoryFile} 2>&1&echo. >{TerminalHelper.EndFile}");
                    while (File.Exists(TerminalHelper.EndFile) == false)
                    {
                        await Task.Delay(100);
                    }
                    DirLabel.Text = Regex.Replace((await FileIO.ReadLinesAsync((await StorageFile.GetFileFromPathAsync(TerminalHelper.DirectoryFile)))).First(), @"\s+", "");
                    TelnetCommandBox.IsHitTestVisible = true;
                    CommandSendBtn.IsEnabled = true;
                }
                catch (Exception ex) { Helper.ThrowException(ex); }
            }
        }
    }
}
