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
using System.IO;
using Windows.Storage;
using CMDInjector_WP8.Resources;

namespace CMDInjector_WP8
{
    public partial class Startup : PhoneApplicationPage
    {
        public Startup()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Initialize();
            CommandBox.TextWrapping = Helper.LocalSettingsHelper.LoadSettings("CommandsTextWrap", false) ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        private async void Initialize()
        {
            try
            {
                if (HomeHelper.IsCMDInjected())
                {
                    if (File.Exists(@"C:\Windows\System32\Startup.bat"))
                    {
                        Helper.CopyFile(@"C:\Windows\System32\Startup.bat", Helper.localFolder.Path + "\\Startup.bat");
                    }
                    else
                    {
                        Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Startup.bat", Helper.localFolder.Path + "\\Startup.bat");
                    }
                    var text = await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Startup.bat"), Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    CommandBox.Text = text;
                    CommandBox.Text += "\r";
                    CommandBox.Text = CommandBox.Text.Remove(CommandBox.Text.LastIndexOf("\r"));
                }
                else
                {
                    CommandBox.IsReadOnly = true;
                    Helper.DisplayMessage1(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, AppResources.StartupPageFailedInfoTitle);
                }
            }
            catch (Exception ex)
            {
                CommandBox.IsReadOnly = false;
                Helper.ThrowException(ex);
            }
        }

        private async void CommandBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommandBtn.IsEnabled = false;
                await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Startup.bat"), CommandBox.Text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n"));
                Helper.CopyFile(Helper.localFolder.Path + "\\Startup.bat", @"C:\Windows\System32\Startup.bat");
                Helper.DisplayMessage1(AppResources.StartupPageSuccessInfoDescription, Helper.SoundHelper.Sound.Alert, AppResources.StartupPageSuccessInfoTitle);
            }
            catch (Exception ex)
            {
                CommandBtn.IsEnabled = true;
                Helper.ThrowException(ex);
            }
        }

        private void CommandBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommandBtn.IsEnabled = true;
        }
    }
}