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
using WUT_WP8.UWP;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.System.Display;
using System.IO;
using WinUniversalTool;
using CMDInjector_WP8.Resources;

namespace CMDInjector_WP8
{
    public partial class Snapper : PhoneApplicationPage
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        StorageFolder SnapPicturesFolder;
        StorageFolder snapVideosFolder;
        DisplayRequest displayRequest;
        string videoFilename;
        bool isConvertInProgress = false;

        public Snapper()
        {
            InitializeComponent();
            FolderGen();
            if (HomeHelper.IsCMDInjected())
            {
                Connect();
            }
            else
            {
                AmountBox.IsReadOnly = true;
                DelayBox.IsReadOnly = true;
                FrameRateBox.IsReadOnly = true;
                BitRateBox.IsReadOnly = true;
                RecordBtn.IsEnabled = false;
                Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (SnapperHelper.isCapturing)
            {
                CaptureBtn.Content = AppResources.SnapperPageButton1Content2;
                AmountBox.IsEnabled = false;
                DelayBox.IsEnabled = false;
                AmountBox.Text = SnapperHelper.shots;
                DelayBox.Text = SnapperHelper.delay;
                RecordBtn.IsEnabled = false;
            }
            else if (SnapperHelper.isRecording)
            {
                RecordBtn.Content = AppResources.SnapperPageButton2Content2;
                FrameRateBox.IsEnabled = false;
                BitRateBox.IsEnabled = false;
                FrameRateBox.Text = SnapperHelper.frame;
                BitRateBox.Text = SnapperHelper.bit;
            }
        }

        private async void FolderGen()
        {
            StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
            StorageFolder videosFolder = KnownFolders.VideosLibrary;
            SnapPicturesFolder = await picturesFolder.CreateFolderAsync("Snapper", CreationCollisionOption.OpenIfExists);
            snapVideosFolder = await videosFolder.CreateFolderAsync("Snapper", CreationCollisionOption.OpenIfExists);
        }

        private void Connect()
        {
            tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 1000000)
            {
                i++;
            }
            if (!tClient.IsConnected || !HomeHelper.IsConnected())
            {
                AmountBox.IsReadOnly = true;
                DelayBox.IsReadOnly = true;
                FrameRateBox.IsReadOnly = true;
                BitRateBox.IsReadOnly = true;
                RecordBtn.IsEnabled = false;
                Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
            }
        }

        private async Task SendCommand(string command)
        {
            await Task.Run(() =>
            {
                tClient.Send(command);
            });
        }

        private void AmountBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AmountBox.Text != string.Empty && DelayBox.Text != string.Empty && !AmountBox.Text.Contains(".") && !DelayBox.Text.Contains(".") && Convert.ToInt32(AmountBox.Text) > 0 && Convert.ToInt32(AmountBox.Text) <= 999 && Convert.ToInt32(DelayBox.Text) > 0 && Convert.ToInt32(DelayBox.Text) <= 60)
            {
                CaptureBtn.IsEnabled = true;
            }
            else
            {
                CaptureBtn.IsEnabled = false;
            }
        }

        private void CaptureBtn_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    if (displayRequest == null)
                    {
                        displayRequest = new DisplayRequest();
                        displayRequest.RequestActive();
                    }
                    if ((CaptureBtn.Content as string) == AppResources.SnapperPageButton1Content1)
                    {
                        SnapperHelper.isCapturing = true;
                        SnapperHelper.shots = AmountBox.Text;
                        SnapperHelper.delay = DelayBox.Text;
                        CaptureBtn.Content = AppResources.SnapperPageButton1Content2;
                        RecordBtn.IsEnabled = false;
                        AmountBox.IsEnabled = false;
                        DelayBox.IsEnabled = false;
                        if (Helper.LocalSettingsHelper.LoadSettings("SnapperNotify", true)) Helper.NotificationHelper.PushNotification(AppResources.SnapperPageNotification1Content, AppResources.MainPageSnapperButtonText);
                        File.Delete($"{Helper.localFolder.Path}\\CaptureStop.txt");
                        await SendCommand("start cmd.exe /c " + Helper.installedLocation.Path + "\\Contents\\BatchScripts\\SnapperShots.bat " + AmountBox.Text + " " + DelayBox.Text + " " + Helper.localFolder.Path);
                    }
                    else
                    {
                        SnapperHelper.isCapturing = false;
                        CaptureBtn.IsEnabled = false;
                        await Helper.localFolder.CreateFileAsync("CaptureStop.txt", CreationCollisionOption.OpenIfExists);
                        CaptureBtn.Content = AppResources.SnapperPageButton1Content1;
                        CaptureBtn.IsEnabled = true;
                        if (BitRateBox.Text != string.Empty && FrameRateBox.Text != string.Empty) RecordBtn.IsEnabled = true;
                        AmountBox.IsEnabled = true;
                        DelayBox.IsEnabled = true;
                        if (Helper.LocalSettingsHelper.LoadSettings("SnapperNotify", true)) Helper.NotificationHelper.PushNotification(AppResources.SnapperPageNotification2Content, AppResources.MainPageSnapperButtonText);
                    }
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                        displayRequest = null;
                    }
                });
            });
        }

        private void FrameRateBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FrameRateBox.Text != string.Empty && BitRateBox.Text != string.Empty && !FrameRateBox.Text.Contains(".") && !BitRateBox.Text.Contains(".") && Convert.ToInt32(FrameRateBox.Text) > 0 && Convert.ToInt32(FrameRateBox.Text) <= 30 && Convert.ToInt32(BitRateBox.Text) > 0 && Convert.ToInt32(BitRateBox.Text) <= 9999)
            {
                if (!SnapperHelper.isCapturing)
                    RecordBtn.IsEnabled = true;
            }
            else
            {
                RecordBtn.IsEnabled = false;
            }
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task.Run(async () =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        if (displayRequest == null)
                        {
                            displayRequest = new DisplayRequest();
                            displayRequest.RequestActive();
                        }
                        if ((RecordBtn.Content as string) == AppResources.SnapperPageButton2Content1)
                        {
                            SnapperHelper.isRecording = true;
                            SnapperHelper.frame = FrameRateBox.Text;
                            SnapperHelper.bit = BitRateBox.Text;
                            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.StartRecord);
                            RecordBtn.Content = AppResources.SnapperPageButton2Content2;
                            CaptureBtn.IsEnabled = false;
                            FrameRateBox.IsEnabled = false;
                            BitRateBox.IsEnabled = false;
                            Helper.NotificationHelper.PushNotification(AppResources.SnapperPageNotification3Content, AppResources.MainPageSnapperButtonText);
                            File.Delete($"{Helper.localFolder.Path}\\RecordStop.txt");
                            StorageFolder ClipsFolder = await Helper.localFolder.CreateFolderAsync("SnapperRecords", CreationCollisionOption.ReplaceExisting);
                            await SendCommand("start cmd.exe /c " + Helper.installedLocation.Path + "\\Contents\\BatchScripts\\SnapperRecords.bat 5000 0 " + Helper.localFolder.Path + " " + ClipsFolder.Path);
                        }
                        else
                        {
                            SnapperHelper.isRecording = false;
                            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.StopRecord);
                            await Helper.localFolder.CreateFileAsync("RecordStop.txt", CreationCollisionOption.OpenIfExists);
                            RecordBtn.Content = AppResources.SnapperPageButton2Content3;
                            RecordBtn.IsEnabled = false;
                            if (AmountBox.Text != string.Empty && DelayBox.Text != string.Empty) CaptureBtn.IsEnabled = true;
                            StorageFolder ClipsFolder = await Helper.localFolder.CreateFolderAsync("SnapperRecords", CreationCollisionOption.OpenIfExists);
                            if (isConvertInProgress == false)
                            {
                                isConvertInProgress = true;
                                IProgress<double> progress = new Progress<double>(value =>
                                {
                                    var finalValue = Math.Round(value);
                                    RecordBtn.Content = $"{AppResources.SnapperPageButton2Content3} {finalValue}%";
                                });
                                videoFilename = await Images2Video.Convert(ClipsFolder, Convert.ToInt32(FrameRateBox.Text), Convert.ToInt32(BitRateBox.Text), snapVideosFolder, progress);
                                isConvertInProgress = false;
                            }
                            while (isConvertInProgress)
                            {
                                await Task.Delay(100);
                            }
                            await ClipsFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                            RecordBtn.Content = AppResources.SnapperPageButton2Content1;
                            RecordBtn.IsEnabled = true;
                            FrameRateBox.IsEnabled = true;
                            BitRateBox.IsEnabled = true;
                            Helper.NotificationHelper.PushNotification($"{AppResources.SnapperPageNotification4Content} {snapVideosFolder.Path}\\{videoFilename}.", AppResources.MainPageSnapperButtonText);
                        }
                        if (displayRequest != null)
                        {
                            displayRequest.RequestRelease();
                            displayRequest = null;
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }
    }
}