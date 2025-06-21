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
using System.Threading;
using System.Threading.Tasks;
using WUT_WP8.UWP;
using System.IO;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using CMDInjector_WP8.Resources;
using System.Xml.Linq;

namespace CMDInjector_WP8
{
    public partial class PacMan : PhoneApplicationPage
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient;
        int succeeded = 0;
        int failed = 0;

        public PacMan()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            BrowseBtnTip.Visibility = Helper.LocalSettingsHelper.LoadSettings("BrowseBtnTipSettings", true) ? Visibility.Visible : Visibility.Collapsed;

            StorageFolder sdCard = (await Helper.externalFolder.GetFoldersAsync()).FirstOrDefault();
            if (sdCard == null)
            {
                Helper.LocalSettingsHelper.SaveSettings("StorageSet", false);
            }
        }

        private async Task Connect()
        {
            tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
            tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 150)
            {
                await Task.Delay(100);
                i++;
            }
            if (!tClient.IsConnected || !HomeHelper.IsConnected())
            {
                InstallBtn.IsEnabled = false;
                BrowseBtn.IsEnabled = false;
                ResultBox.Text = $"Error: {HomeHelper.GetTelnetTroubleshoot()}";
            }
        }

        private async Task SendCommand(string command)
        {
            if (File.Exists($"{PacManHelper.InstallEndFile}")) File.Delete($"{PacManHelper.InstallEndFile}");
            if (File.Exists($"{PacManHelper.MoveEndFile}")) File.Delete($"{PacManHelper.MoveEndFile}");
            await Task.Run(() =>
            {
                tClient.Send(command);
            });
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();
                if (RegisterType.IsChecked == true)
                {
                    picker.FileTypeFilter.Add(".xml");
                }
                else
                {
                    picker.FileTypeFilter.Add(".xap");
                    picker.FileTypeFilter.Add(".appx");
                    picker.FileTypeFilter.Add(".appxbundle");
                }
                CoreApplication.GetCurrentView().Activated += BrowseClick;
                picker.PickMultipleFilesAndContinue();
            }
            catch
            {
                //CommonHelper.ThrowException(ex);
            }
        }

        private void BrowseBtn_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();
                if (RegisterType.IsChecked == true)
                {
                    picker.FileTypeFilter.Add(".xml");
                }
                else
                {
                    picker.FileTypeFilter.Add(".xap");
                    picker.FileTypeFilter.Add(".appx");
                    picker.FileTypeFilter.Add(".appxbundle");
                }
                CoreApplication.GetCurrentView().Activated += BrowseHold;
                picker.PickMultipleFilesAndContinue();
                Helper.LocalSettingsHelper.SaveSettings("BrowseBtnTipSettings", false);
            }
            catch
            {
                //CommonHelper.ThrowException(ex);
            }
        }

        private void BrowseClick(CoreApplicationView sender, IActivatedEventArgs args)
        {
            var continuationEventArgs = args as FileOpenPickerContinuationEventArgs;
            if (continuationEventArgs != null)
            {
                var files = continuationEventArgs.Files;
                if (files.Count > 0)
                {
                    AppsPath.Text = string.Empty;
                    foreach (var file in files)
                    {
                        AppsPath.Text += file.Path + ";";
                    }
                    AppsPath.Text = AppsPath.Text.Remove(AppsPath.Text.Length - 1, 1);
                }
            }
            CoreApplication.GetCurrentView().Activated -= BrowseClick;
        }

        private void BrowseHold(CoreApplicationView sender, IActivatedEventArgs args)
        {
            var continuationEventArgs = args as FileOpenPickerContinuationEventArgs;
            if (continuationEventArgs != null)
            {
                var files = continuationEventArgs.Files;
                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (string.IsNullOrWhiteSpace(AppsPath.Text) || string.IsNullOrEmpty(AppsPath.Text))
                        {
                            AppsPath.Text += file.Path + ";";
                        }
                        else
                        {
                            AppsPath.Text += ";" + file.Path + ";";
                        }
                        AppsPath.Text = AppsPath.Text.Remove(AppsPath.Text.Length - 1, 1);
                    }
                }
            }
            CoreApplication.GetCurrentView().Activated -= BrowseHold;
        }

        private void DeploymentInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            Helper.DisplayMessage1(AppResources.PacManPageDeploymentInfoDescription, Helper.SoundHelper.Sound.Alert, AppResources.PacManPageDeploymentInfoTitle);
        }

        private void AppsPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AppsPath.Text == string.Empty)
            {
                InstallBtn.IsEnabled = false;
                AppsPath.TextWrapping = TextWrapping.NoWrap;
            }
            else
            {
                InstallBtn.IsEnabled = true;
                AppsPath.TextWrapping = TextWrapping.Wrap;
            }
        }

        private async void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                PacManHelper.installOnProcess = true;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        SeeLogBox.Visibility = Visibility.Collapsed;
                        DeploymentOpt.IsEnabled = false;
                        DeploymentInfoBtn.IsEnabled = false;
                        InstallType.IsEnabled = false;
                        UpdateType.IsEnabled = false;
                        RegisterType.IsEnabled = false;
                        InstallBtn.IsEnabled = false;
                        BrowseBtn.IsEnabled = false;
                        AppsPath.IsEnabled = false;
                        InstallProg.Value = 0;
                        InstallProg.Maximum = AppsPath.Text.Split(';').Length;
                        IndivitualInstProg.IsIndeterminate = true;
                        IndivitualInstProg.Visibility = Visibility.Visible;
                        int appCount = 0;
                        succeeded = 0;
                        failed = 0;
                        List<string> appxs = new List<string>();
                        if (File.Exists($"{PacManHelper.InstallEndFile}"))
                            File.Delete($"{PacManHelper.InstallEndFile}");
                        if (HomeHelper.IsCMDInjected()) await Connect();
                        await PacManHelper.CreateLogFile();
                        foreach (var appPath in AppsPath.Text.Split(';'))
                        {
                            string report = string.Empty;
                            string selectedType = string.Empty;
                            string selectedOption = string.Empty;
                            string selectedStorage = string.Empty;
                            var appName = Path.GetFileName(appPath);
                            ResultBox.Text = $"[{++appCount}/{AppsPath.Text.Split(';').Length}] {report} \"{appName}\".";
                            string option = string.Empty;
                            if (Helper.LocalSettingsHelper.LoadSettings("StorageSet", false))
                                option = "dis";
                            else
                                option = "di";
                            if (InstallType.IsChecked == true && DeploymentOpt.SelectedIndex != 0)
                            {
                                if (Helper.LocalSettingsHelper.LoadSettings("StorageSet", false))
                                    option = "is";
                                else
                                    option = "i";
                            }
                            else if (UpdateType.IsChecked == true)
                            {
                                if (DeploymentOpt.SelectedIndex == 0)
                                    option = "uda";
                                else
                                    option = "ua";
                            }
                            else if (RegisterType.IsChecked == true)
                            {
                                option = "de";
                            }
                            if (DeploymentOpt.SelectedIndex == 1 && appPath.ToLower().EndsWith(".appx") && Helper.Archive.CheckFileExist(appPath, "AppxManifest.xml", false))
                            {
                                var val = await Helper.Archive.ReadTextFromZip(appPath, "AppxManifest.xml");
                                XDocument doc = XDocument.Parse(val);
                                XNamespace mp = "http://schemas.microsoft.com/appx/2014/phone/manifest";
                                string productId = doc.Root.Element(mp + "PhoneIdentity").Attribute("PhoneProductId").Value;
                                await SendCommand($"TH.exe \"{appPath}\" -e \"C:\\Data\\Programs\\{{{productId}}}\"&del \"C:\\Data\\Programs\\{{{productId}}}\\AppxSignature.p7x\"&TH.exe \"C:\\Data\\Programs\\{{{productId}}}\\AppxManifest.xml\" -de >{PacManHelper.InstallResultFile} 2>&1&echo. >{PacManHelper.InstallEndFile}");
                            }
                            else if (DeploymentOpt.SelectedIndex == 1 && appPath.ToLower().EndsWith(".appxbundle") && PacManHelper.CheckFileExistInPackage(appPath, "appx"))
                            {
                                appxs = PacManHelper.ExtractAppxFromBundle(appPath, Helper.cacheFolder.Path);
                                foreach (var appx in appxs)
                                {
                                    if (PacManHelper.CheckFileExistInPackage($"{Helper.cacheFolder.Path}\\{appx}", "exe") && Helper.Archive.CheckFileExist($"{Helper.cacheFolder.Path}\\{appx}", "AppxManifest.xml", false))
                                    {
                                        var val = await Helper.Archive.ReadTextFromZip($"{Helper.cacheFolder.Path}\\{appx}", "AppxManifest.xml");
                                        XDocument doc = XDocument.Parse(val);
                                        XNamespace mp = "http://schemas.microsoft.com/appx/2014/phone/manifest";
                                        string productId = doc.Root.Element(mp + "PhoneIdentity").Attribute("PhoneProductId").Value;
                                        await SendCommand($"TH.exe \"{$"{Helper.cacheFolder.Path}\\{appx}"}\" -e \"C:\\Data\\Programs\\{{{productId}}}\"&del \"C:\\Data\\Programs\\{{{productId}}}\\AppxSignature.p7x\"&TH.exe \"C:\\Data\\Programs\\{{{productId}}}\\AppxManifest.xml\" -de >{PacManHelper.InstallResultFile} 2>&1&echo. >{PacManHelper.InstallEndFile}");
                                    }
                                }
                            }
                            else
                            {
                                await SendCommand($"TH.exe \"{appPath}\" -{option} >{PacManHelper.InstallResultFile} 2>&1&echo. >{PacManHelper.InstallEndFile}");
                            }
                            while (!File.Exists($"{PacManHelper.InstallEndFile}"))
                            {
                                await Task.Delay(500);
                            }
                            foreach (var appx in appxs)
                                File.Delete($"{Helper.cacheFolder.Path}\\{appx}");
                            string result = await FileIO.ReadTextAsync(await PacManHelper.GetInstallResultFile());
                            if (result.Contains("TH: Completed Successfully."))
                            {
                                succeeded++;
                            }
                            else
                            {
                                failed++;
                                try
                                {
                                    string errorCode = "<PACMAN ERROR CODE>";
                                    string errorText = "<PACMAN ERROR DESCRIPTION>";
                                    string[] error = (await FileIO.ReadLinesAsync(await StorageFile.GetFileFromPathAsync($"{PacManHelper.InstallResultFile}"))).ToArray();
                                    foreach (var errorLine in error)
                                    {
                                        if (result.Contains("PacMan Error Code"))
                                        {
                                            if (errorLine.Contains("PacMan Error Code"))
                                            {
                                                errorCode = errorLine.Split('=')[1].Replace(" ", "");
                                            }
                                            else if (errorLine.Contains("PacMan Error Description"))
                                            {
                                                errorText = errorLine.Split('=')[1].Replace("PM Error: ", "").Replace("Error: ", "");
                                                errorText = errorText.Remove(errorText.IndexOf(" "), " ".Length).EndsWith(".") ? errorText.Remove(errorText.IndexOf(" "), " ".Length).Remove(errorText.Remove(errorText.IndexOf(" "), " ".Length).Length - 1) : errorText.Remove(errorText.IndexOf(" "), " ".Length);
                                            }
                                        }
                                        else if (result.Contains("error code"))
                                        {
                                            if (errorLine.Contains("error code"))
                                            {
                                                if (errorLine.Split('.').Length == 2)
                                                {
                                                    errorText = UpdateType.IsChecked == true ? "Could not update the application" : "Could not install the application";
                                                    int pFrom = errorLine.Split('.')[1].IndexOf("  ") + "  ".Length;
                                                    int pTo = errorLine.Split('.')[1].LastIndexOf(", ");
                                                    errorCode = errorLine.Split('.')[1].Substring(pFrom, pTo - pFrom).Replace("error code = ", " ");
                                                }
                                                else
                                                {
                                                    errorText = errorLine.Split('.')[0].Replace("ERROR: ", "");
                                                    int pFrom = errorLine.Split('.')[2].IndexOf("  ") + "  ".Length;
                                                    int pTo = errorLine.Split('.')[2].LastIndexOf(", ");
                                                    errorCode = errorLine.Split('.')[2].Substring(pFrom, pTo - pFrom).Replace("error code = ", " ");
                                                }
                                            }
                                        }
                                        else if (result.Contains("dwCode"))
                                        {
                                            if (errorLine.Contains("dwCode"))
                                            {
                                                errorCode = result.Split('=')[2].Split('\n')[0].Replace("\r", "");
                                            }
                                        }
                                        else if (result.Contains("ERROR"))
                                        {
                                            errorText = result.Split('.')[0].Replace("ERROR: ", "").Replace("\r\n", "").Replace("\r", "");
                                            errorCode = result.Split('.')[1].Replace(" hr = ", "").Replace("\r\n", "").Replace("\r", "");
                                        }
                                    }
                                    await PacManHelper.WriteLogFile($"{AppResources.PacManPageLogFileDescription2}: {appName}\n{AppResources.PacManPageLogFileDescription3}: {errorCode} {errorText}.\n\n");
                                }
                                catch (Exception ex)
                                {
                                    Helper.ThrowException(ex);
                                }
                            }
                            InstallProg.Value = appCount;
                        }
                        ResultBox.Text = $"{AppResources.PacManPageResultText3}: {succeeded} {AppResources.PacManPageResultText4}, {failed} {AppResources.PacManPageResultText5}.";
                        if (failed != 0)
                        {
                            SeeLogBox.Visibility = Visibility.Visible;
                            ShowLog();
                        }
                        IndivitualInstProg.Visibility = Visibility.Collapsed;
                        IndivitualInstProg.IsIndeterminate = false;
                        DeploymentOpt.IsEnabled = true;
                        DeploymentInfoBtn.IsEnabled = true;
                        InstallType.IsEnabled = true;
                        UpdateType.IsEnabled = true;
                        RegisterType.IsEnabled = true;
                        InstallBtn.IsEnabled = true;
                        BrowseBtn.IsEnabled = true;
                        AppsPath.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        Helper.ThrowException(ex);
                    }
                });
                PacManHelper.installOnProcess = false;
            });
        }

        private async void ShowLog()
        {
            string logPath = (await (await PacManHelper.GetLogPath()).GetFileAsync(PacManHelper.LogFileName)).Path;
            string logContent = await PacManHelper.ReadLogFile();
            if (failed > 1)
            {
                Helper.DisplayMessage1($"{AppResources.PacManPageLogFileDescription1}: " + logPath + "\n\n" + logContent, Helper.SoundHelper.Sound.Alert, AppResources.PacManPageLogFileTitle1);
            }
            else
            {
                Helper.DisplayMessage1($"{AppResources.PacManPageLogFileDescription1}: " + logPath + "\n\n" + logContent, Helper.SoundHelper.Sound.Alert, AppResources.PacManPageLogFileTitle2);
            }
        }

        private void InstallType_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InstallType.IsChecked == true)
                {
                    InstallBtn.Content = AppResources.PacManPageButton2Text1;
                }
                else if (UpdateType.IsChecked == true)
                {
                    InstallBtn.Content = AppResources.PacManPageButton2Text2;
                }
                else if (RegisterType.IsChecked == true)
                {
                    InstallBtn.Content = AppResources.PacManPageButton2Text3;
                }
            }
            catch
            {
                //CommonHelper.ThrowException(ex);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            StorageFolder sdCard = (await Helper.externalFolder.GetFoldersAsync()).FirstOrDefault();
            if (sdCard == null)
            {
                Helper.LocalSettingsHelper.SaveSettings("StorageSet", false);
            }
        }

        private void SeeLog_Click(object sender, RoutedEventArgs e)
        {
            ShowLog();
        }
    }
}