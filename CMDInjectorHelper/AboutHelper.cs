using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Web.Http;

namespace CMDInjectorHelper
{
    public static class AboutHelper
    {
        private static async Task<dynamic> GetLatestVersion() => await Task.Run(async () =>
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537");
            HttpResponseMessage response = await client.GetAsync(new Uri("https://api.github.com/repos/fadilfadz01/CMD.Injector_WP8/releases/latest"));
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return Helper.Json.DeserializeObject(responseBody);
            }
            return null;
        });

        public static async Task<dynamic> IsNewUpdateAvailable()
        {
            var release = await GetLatestVersion();
            if (release != null)
            {
                string latestReleaseVersion = release.tag_name;
                PackageVersion version = Package.Current.Id.Version;

                string current = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

                latestReleaseVersion = latestReleaseVersion.Replace("v", "").Replace("V", "");

                var newVersionRequested = Helper.IsStrAGraterThanStrB(latestReleaseVersion, current, '.');
                if (newVersionRequested)
                {
                    return release;
                }
            }
            return null;
        }

        public static async Task<string> GetLatestReleaseNote()
        {
            var release = await GetLatestVersion();
            if (release != null)
                return release.body;

            return null;
        }

        public static async Task<StorageFolder> GetDownloadsFolder()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("DownloadsFolder"))
            {
                try
                {
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("DownloadsFolder");
                }
                catch (Exception ex)
                {
                    //Helper.ThrowException(ex);
                }
            }
            StorageFolder returnFolder = null;
            var result = await Helper.DisplayMessage2("Please select the Downloads folder.", Helper.SoundHelper.Sound.Alert, "Browse", "Select Location");
            if (result == 1)
            {
                bool completed = false;
                var folderPicker = new FolderPicker
                {
                    SuggestedStartLocation = PickerLocationId.Downloads
                };
                folderPicker.FileTypeFilter.Add("*");
                CoreApplication.GetCurrentView().Activated += (s, e) =>
                {
                    var continuationEventArgs = e as FolderPickerContinuationEventArgs;
                    if (continuationEventArgs != null)
                    {
                        var folder = continuationEventArgs.Folder;
                        if (folder != null)
                        {
                            StorageApplicationPermissions.FutureAccessList.AddOrReplace("DownloadsFolder", folder);
                            returnFolder = folder;
                        }
                    }
                    completed = true;
                };
                folderPicker.PickFolderAndContinue();
                while (!completed)
                {
                    await Task.Delay(200);
                }
            }
            return returnFolder;
        }
    }
}
