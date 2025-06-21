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
using Microsoft.Phone.Tasks;
using CMDInjector_WP8.Resources;
using System.Threading.Tasks;
using Windows.Storage;
using System.Threading;
using WUT_WP8.UWP;
using System.Windows.Media.Imaging;

namespace CMDInjector_WP8
{
    public partial class About : PhoneApplicationPage
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient;

        public About()
        {
            InitializeComponent();
            versionText.Text = $"CMD Injector v{Helper.currentVersion}";
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

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        private void ChangelogBtn_Click(object sender, RoutedEventArgs e)
        {
            Changelog.DisplayLog();
        }

        private void PermissionHelp_Click(object sender, RoutedEventArgs e)
        {
            Helper.DisplayMessage1(" • Access your Internet connection\n" +
                    "Check the App update and download if available.\n\n" +
                    /*" • Use your documents library\n" +
                    "Store the PacMan installer log to the documents directory.\n\n" +
                    " • Use your pictures library\n" +
                    "Save the Snapper shots to the pictures directory.\n\n" +
                    " • Use your video library\n" +
                    "Save the Snapper clips to the videos directory.\n\n" +*/
                    " • Use data stored on an external storage device\n" +
                    "Check SD card availability and install apps to the SD Card storage.\n\n" +
                    /*" • Gather information about other apps\n" +
                    "Used by the PacMan manager to display the installed apps information.\n\n" +
                    " • Manage other apps directly\n" +
                    "Used by the PacMan manager to launch, uninstall apps etc...\n\n" +
                    " • extendedBackgroundTaskTime\n" +
                    "Run Snapper screen capturer and recorder in the background.\n\n" +
                    " • Begin an unconstrained extended execution session\n" +
                    "Same as the above capability extendedBackgroundTaskTime.\n\n" +
                    " • id_cap_oempublicdirectory\n" +
                    "Copy the required CMD components to the ..\\OEM\\Public directory on injection.\n\n" +*/
                    " • id_cap_runtime_config\n" +
                    "Read and write the registry and CMD components.\n\n" +
                    " • id_cap_oem_custom\n" +
                    "same as the above capability id_cap_runtime_config.\n\n" +
                    /*" • id_cap_chamber_profile_code_rw\n" +
                    "Used by the PacMan manager to backup and move the apps package, and the TweakBox search options to read the apps AppxManifest.xml file and get the ProductID.\n\n" +
                    " • id_cap_chamber_profile_data_rw\n" +
                    "Used by the PacMan manager to backup the apps data.\n\n" +*/
                    " • id_cap_public_folder_full\n" +
                    "Access ..\\Data\\Users\\Public location, and store the PacMan installer log to the documents directory.\n\n", Helper.SoundHelper.Sound.Alert, AppResources.AboutPageSubTitle3Description3);
        }

        private void Telegram_Click(object sender, RoutedEventArgs e)
        {
            LaunchWebUri("https://t.me/fadilfadz01");
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            LaunchWebUri("https://github.com/fadilfadz01");
        }

        private void Youtube_Click(object sender, RoutedEventArgs e)
        {
            LaunchWebUri("https://www.youtube.com/channel/UCe-pdxB7iM6i__rIkRf7Kkg");
        }

        private void Paypal_Click(object sender, RoutedEventArgs e)
        {
            LaunchWebUri("https://www.paypal.me/fadilfadz01");
        }

        private void Boosty_Click(object sender, RoutedEventArgs e)
        {
            LaunchWebUri("https://boosty.to/fadilfadz01/donate");
        }

        private void Upi_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox customMessageBox = new CustomMessageBox
            {
                Title = "UPI",
                LeftButtonContent = "OK",
                Content = new Image
                {
                    Margin = new Thickness(10,10,20,10),
                    Source = new BitmapImage(new Uri("Assets/Images/DonationUPI.jpg", UriKind.RelativeOrAbsolute))
                }
            };
            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
            customMessageBox.Show();
        }

        private void LaunchWebUri(string url)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(url, UriKind.Absolute);
            webBrowserTask.Show();
        }

        private async void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        UpdateBtn.IsEnabled = false;
                        UpdateBtn.Content = AppResources.AboutPageButton1Text2;
                        var latestRelease = await AboutHelper.IsNewUpdateAvailable();
                        if (latestRelease == null)
                        {
                            Helper.NotificationHelper.PushNotification(AppResources.AboutPageToastContent1, AppResources.AboutPageToastTitle1);
                            UpdateBtn.IsEnabled = true;
                            UpdateBtn.Content = AppResources.AboutPageButton1Text1;
                            return;
                        }
                        UpdateBtn.Content = AppResources.AboutPageButton1Text1;
                        UpdateBtn.IsEnabled = true;
                        double megaSize = ConvertBytesToMegabytes((long)latestRelease.assets[0].size);
                        var result = await Helper.DisplayMessage2($"{latestRelease.name}\n\nChanges:\n{await AboutHelper.GetLatestReleaseNote()}\n\nPackage: {latestRelease.assets[0].name}\nType: {latestRelease.assets[0].content_type}\nSize: {String.Format("{0:0.000}", megaSize)}MB", Helper.SoundHelper.Sound.Alert, "Update Available", "Cancel", true, "Update");
                        if (result != 0)
                        {
                            return;
                        }
                        StorageFolder downloadFolder = await AboutHelper.GetDownloadsFolder();
                        if (downloadFolder == null)
                        {
                            Helper.DisplayMessage1(AppResources.AboutPageMessageBox1Content, Helper.SoundHelper.Sound.Error, AppResources.AboutPageMessageBox1Title);
                            return;
                        }
                        Connect();
                        StorageFile updatePackage = await downloadFolder.CreateFileAsync((string)latestRelease.assets[0].name, CreationCollisionOption.ReplaceExisting);
                        Uri sourceFile = new Uri((string)latestRelease.assets[0].browser_download_url);
                        UpdateBtn.Content = AppResources.AboutPageButton1Text3;
                        UpdateBtn.IsEnabled = false;
                        IProgress<int> progress = new Progress<int>(async value =>
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                if (value == 100)
                                {
                                    Helper.NotificationHelper.PushNotification(AppResources.AboutPageToastContent2 + " " + updatePackage.Path + ".", AppResources.AboutPageToastTitle2);

                                }
                                else if (value == -100)
                                {
                                    Helper.NotificationHelper.PushNotification(AppResources.AboutPageToastContent3, AppResources.AboutPageToastTitle3);
                                }
                                else if (value == -50)
                                {
                                    UpdateBtn.Content = AppResources.AboutPageButton1Text4;
                                    Helper.NotificationHelper.PushNotification(AppResources.AboutPageToastContent4, AppResources.AboutPageToastTitle4);
                                }
                                else
                                {
                                    double doubleValue = Convert.ToDouble("0." + value);
                                    UpdateBtn.Content = $"{AppResources.AboutPageButton1Text5} {value}%";
                                }
                            });
                        });
                        await Helper.DownloadFile(sourceFile, updatePackage, progress);
                        if (tClient.IsConnected && HomeHelper.IsConnected())
                        {
                            await Task.Delay(TimeSpan.FromSeconds(2));
                            tClient.Send($"start TH.exe \"{updatePackage.Path}\" -di");
                        }
                        else
                        {
                            Helper.DisplayMessage1(AppResources.AboutPageMessageBox2Content, Helper.SoundHelper.Sound.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Excep_FromHResult 0x80072EE4")
                        {
                            Helper.NotificationHelper.PushNotification(AppResources.AboutPageToastContent5, AppResources.AboutPageToastTitle5);
                        }
                        else
                        {
                            Helper.NotificationHelper.PushNotification(AppResources.AboutPageToastContent6, AppResources.AboutPageToastTitle6);
                            //Helper.ThrowException(ex);
                        }
                    }
                    UpdateBtn.Content = AppResources.AboutPageButton1Text1;
                    UpdateBtn.IsEnabled = true;
                });
            });
        }
    }
}