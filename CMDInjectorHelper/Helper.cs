using Coding4Fun.Toolkit.Controls;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ndtklib;
using Newtonsoft.Json;
using OEMSharedFolderAccessLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using WUT_WP8.UWP;

namespace CMDInjectorHelper
{
    public static class Helper
    {
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);

        public static int currentBatchVersion = Convert.ToInt32(string.Format("{0}{1}{2}{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision));
        public static string InjectedBatchVersion { get { return File.Exists(@"C:\Windows\System32\CMDInjectorVersion.dat") ? new StreamReader(@"C:\Windows\System32\CMDInjectorVersion.dat").ReadToEnd() : "0000"; } }
        public static string currentVersion = string.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
        public static string build = $"6.3.{Environment.OSVersion.Version.Build}";
        public static StorageFolder installedLocation = Package.Current.Installed­Location;
        public static StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public static StorageFolder cacheFolder = ApplicationData.Current.LocalCacheFolder;
        public static StorageFolder externalFolder = KnownFolders.RemovableDevices;

        private static COEMSharedFolder oem = new COEMSharedFolder();
        private static NRPC rpc = new NRPC();

        private static async void Connect()
        {
            tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 150)
            {
                await Task.Delay(100);
                i++;
            }
        }

        public static void Init()
        {
            oem.RPC_Init();
            rpc.Initialize();
            Connect();
        }

        public static MessageBoxResult DisplayMessage1(string content, SoundHelper.Sound sound = SoundHelper.Sound.Alert, string title = "", MessageBoxButton messageBoxButton = MessageBoxButton.OK)
        {
            SoundHelper.PlaySound(sound);
            return MessageBox.Show(content, title, messageBoxButton);
        }

        public static async Task<int?> DisplayMessage2(string content, SoundHelper.Sound sound = SoundHelper.Sound.Alert, string title = "", string buttonText = "Close", bool seconadaryButton = false, string seconadaryButtonText = "Ok")
        {
            try
            {
                int? res = null;
                CustomMessageBox messageBox = new CustomMessageBox();
                messageBox.Caption = title;
                messageBox.Message = content;
                if (seconadaryButton)
                    messageBox.LeftButtonContent = seconadaryButtonText;
                else
                    messageBox.IsLeftButtonEnabled = false;
                messageBox.RightButtonContent = buttonText;
                messageBox.Dismissed += (s, e) =>
                {
                    if (e.Result == CustomMessageBoxResult.LeftButton)
                        res = 0;
                    else
                        res = 1;
                };
                SoundHelper.PlaySound(sound);
                messageBox.Show();
                while (res == null)
                {
                    await Task.Delay(200);
                }
                return res;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public static void ThrowException(Exception ex)
        {
            DisplayMessage1(ex.Message + "\n" + ex.StackTrace, SoundHelper.Sound.Error, "Exception");
        }

        public static bool IsStrAGraterThanStrB(string strA, string strB, char separator)
        {
            var result = false;
            var stringA = strA.Trim().Split(separator);
            var stringB = strB.Trim().Split(separator);

            int length;
            if (stringA.Length > stringB.Length)
            {
                length = stringA.Length;
            }
            else
            {
                length = stringB.Length;
            }

            for (int i = 0; i < length; i++)
            {
                if (int.Parse(stringA[i]) > int.Parse(stringB[i]))
                {
                    result = true;
                    break;
                }
                else if (int.Parse(stringA[i]) < int.Parse(stringB[i]))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        /*public static async Task<string> ReadLine(string file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                    return Regex.Replace(line, @"\s+", "");
            }
            return string.Empty;
        }*/

        public static uint CopyFile(string source, string destination)
        {
            return rpc.FileCopy(source, destination, 0);
        }

        public static uint RebootSystem()
        {
            return rpc.SystemReboot();
        }

        public static async Task DownloadFile(Uri downloadURL, StorageFile file, IProgress<int> progression)
        {
            DownloadOperation downloadOperation;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            BackgroundDownloader backgroundDownloader = new BackgroundDownloader();

            //Set url and file
            downloadOperation = backgroundDownloader.CreateDownload(downloadURL, file);

            Progress<DownloadOperation> progress = new Progress<DownloadOperation>((dOperation) =>
            {
                var _total = (long)dOperation.Progress.TotalBytesToReceive;
                var _received = (long)dOperation.Progress.BytesReceived;
                int _progress = (int)(100 * (double)(_received / (double)_total));
                switch (dOperation.Progress.Status)
                {
                    case BackgroundTransferStatus.Running:
                        //Update your progress here
                        progression.Report(_progress);
                        break;
                    case BackgroundTransferStatus.Error:
                        //An error occured while downloading
                        progression.Report(-100);
                        break;
                    case BackgroundTransferStatus.PausedNoNetwork:
                        //No network detected
                        progression.Report(-50);
                        break;
                    case BackgroundTransferStatus.PausedCostedNetwork:
                        //Download paused because of metered connection
                        break;
                    case BackgroundTransferStatus.PausedByApplication:
                        //Download paused
                        break;
                    case BackgroundTransferStatus.Canceled:
                        //Download canceled
                        progression.Report(-100);
                        break;
                }
                if (_progress >= 100)
                {
                    //Download Done
                    dOperation = null;
                }
            });

            await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
        }

        public static class LocalSettingsHelper
        {
            public static void SaveSettings(string key, bool updateValue)
            {
                ApplicationData.Current.LocalSettings.Values[key] = updateValue;
            }

            public static void SaveSettings(string key, DateTime updateValue)
            {
                ApplicationData.Current.LocalSettings.Values[key] = updateValue;
            }

            public static void SaveSettings(string key, decimal updateValue)
            {
                ApplicationData.Current.LocalSettings.Values[key] = updateValue;
            }

            public static void SaveSettings(string key, double updateValue)
            {
                ApplicationData.Current.LocalSettings.Values[key] = updateValue;
            }

            public static void SaveSettings(string key, float updateValue)
            {
                ApplicationData.Current.LocalSettings.Values[key] = updateValue;
            }

            public static void SaveSettings(string key, int updateValue)
            {
                ApplicationData.Current.LocalSettings.Values[key] = updateValue;
            }

            public static void SaveSettings(string key, long updateValue)
            {
                ApplicationData.Current.LocalSettings.Values[key] = updateValue;
            }

            public static void SaveSettings(string key, string updateValue)
            {
                ApplicationData.Current.LocalSettings.Values[key] = updateValue;
            }

            public static bool LoadSettings(string key, bool defaultValue)
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    return (bool)ApplicationData.Current.LocalSettings.Values[key];
                }
                return defaultValue;
            }

            public static DateTime LoadSettings(string key, DateTime defaultValue)
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    return (DateTime)ApplicationData.Current.LocalSettings.Values[key];
                }
                return defaultValue;
            }

            public static decimal LoadSettings(string key, decimal defaultValue)
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    return (decimal)ApplicationData.Current.LocalSettings.Values[key];
                }
                return defaultValue;
            }

            public static double LoadSettings(string key, double defaultValue)
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    return (double)ApplicationData.Current.LocalSettings.Values[key];
                }
                return defaultValue;
            }

            public static float LoadSettings(string key, float defaultValue)
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    return (float)ApplicationData.Current.LocalSettings.Values[key];
                }
                return defaultValue;
            }

            public static int LoadSettings(string key, int defaultValue)
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    return (int)ApplicationData.Current.LocalSettings.Values[key];
                }
                return defaultValue;
            }

            public static long LoadSettings(string key, long defaultValue)
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    return (long)ApplicationData.Current.LocalSettings.Values[key];
                }
                return defaultValue;
            }

            public static string LoadSettings(string key, string defaultValue)
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    return (string)ApplicationData.Current.LocalSettings.Values[key];
                }
                return defaultValue;
            }
        }

        public static class RegistryHelper
        {
            public enum RegistryHive
            {
                HKEY_CLASSES_ROOT = 0,
                HKEY_LOCAL_MACHINE = 1,
                HKEY_CURRENT_USER = 2,
                HKEY_CURRENT_CONFIG = 3,
                HKEY_USERS = 4,
                HKEY_PERFORMANCE_DATA = 5,
                HKEY_DYN_DATA = 6,
                HKEY_CURRENT_USER_LOCAL_SETTINGS = 7
            }

            public enum RegistryType
            {
                REG_SZ = 1,
                REG_EXPAND_SZ = 2,
                REG_BINARY = 3,
                REG_DWORD = 4,
                REG_DWORD_BIG_ENDIAN = 5,
                REG_LINK = 6,
                REG_MULTI_SZ = 7,
                REG_RESOURCE_LIST = 8,
                REG_FULL_RESOURCE_DESCRIPTOR = 9,
                REG_RESOURCE_REQUIREMENTS_LIST = 10,
                REG_QWORD = 11,
            }

            public static string GetRegValue(RegistryHive hKey, string subKey, string value, RegistryType type)
            {
                return oem.rget((uint)hKey, subKey, value, (uint)type);
            }

            public static void SetRegValue(RegistryHive hKey, string subKey, string value, RegistryType type, string buffer)
            {
                oem.rset((uint)hKey, subKey, value, (uint)type, buffer, 0);
            }

            public static uint SetRegValueEx(RegistryHive hKey, string subKey, string value, RegistryType type, string buffer)
            {
                byte[] byteArr = null;
                switch (type)
                {
                    case RegistryType.REG_SZ:
                        byteArr = Encoding.Unicode.GetBytes(buffer + '\0');
                        break;
                    case RegistryType.REG_EXPAND_SZ:
                        byteArr = Encoding.Unicode.GetBytes(buffer + '\0');
                        break;
                    case RegistryType.REG_BINARY:
                        byteArr = StringToByteArrayFastest(buffer);
                        break;
                    case RegistryType.REG_DWORD:
                        byteArr = BitConverter.GetBytes(uint.Parse(buffer));
                        break;
                    case RegistryType.REG_MULTI_SZ:
                        byteArr = Encoding.Unicode.GetBytes(buffer + '\0');
                        break;
                }
                return rpc.RegSetValue((uint)hKey, subKey, value, (uint)type, byteArr);
            }

            public static uint SetRegBinaryValueEx(RegistryHive hKey, string subKey, string value, int buffer)
            {
                var byteArr = BitConverter.GetBytes(buffer);
                return rpc.RegSetValue((uint)hKey, subKey, value, (uint)RegistryType.REG_BINARY, byteArr);
            }

            private static int GetHexVal(char hex)
            {
                var val = hex;
                //For uppercase A-F letters:
                //return val - (val < 58 ? 48 : 55);
                //For lowercase a-f letters:
                //return val - (val < 58 ? 48 : 87);
                //Or the two combined, but a bit slower:
                return val - (val < 58 ? 48 : val < 97 ? 55 : 87);
            }

            private static byte[] StringToByteArrayFastest(string hex)
            {
                try
                {
                    if (hex.Length % 2 == 1)
                    {
                        throw new Exception("The binary key cannot have an odd number of digits");
                    }
                    var arr = new byte[hex.Length >> 1];
                    for (int i = 0; i < hex.Length >> 1; ++i)
                    {
                        arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
                    }
                    return arr;
                }
                catch (Exception ex)
                {
                    ThrowException(ex);
                    return null;
                }
            }
        }

        public static class NotificationHelper
        {
            public static void PushNotification(string content, string title)
            {
                ToastPrompt toastPrompt = new ToastPrompt
                {
                    Title = title,
                    Message = content,
                    TextOrientation = Orientation.Vertical,
                    TextWrapping = TextWrapping.Wrap
                };
                toastPrompt.Show();
            }

            /*public static bool IsToastAlreadyThere(string tag)
            {
                var toastsHistory = ToastNotificationManager.History.GetHistory();
                foreach (var toastHistory in toastsHistory)
                {
                    if (toastHistory.Tag == tag)
                    {
                        return true;
                    }
                }
                return false;
            }*/
        }

        public static class SoundHelper
        {
            public enum Sound
            {
                Alert,
                Capture,
                Error,
                Lock,
                StartRecord,
                StopRecord,
                Tick
            }

            public static void PlaySound(Sound sound)
            {
                FrameworkDispatcher.Update();
                Stream stream = TitleContainer.OpenStream($"Assets/Sounds/{sound.ToString()}.wav");
                SoundEffect beep = SoundEffect.FromStream(stream);
                beep.Play();
            }
        }

        public static class Archive
        {
            /*public static void CreateZip(string sourceFolder, string destinationFile, CompressionLevel level, bool includeBaseFolder = false, bool overWrite = false)
            {
                if (overWrite && File.Exists(destinationFile)) File.Delete(destinationFile);
                ZipFile.CreateFromDirectory(sourceFolder, destinationFile, level, includeBaseFolder);
            }*/

            public static void ExtractZipWithProgress(string zipFilePath, string destinationFolder, string password = null, IProgress<double> progress = null)
            {
                ZipFile file = null;
                try
                {
                    FileStream fs = File.OpenRead(zipFilePath);
                    file = new ZipFile(fs);

                    if (!string.IsNullOrEmpty(password))
                    {
                        // AES encrypted entries are handled automatically
                        file.Password = password;
                    }

                    long totalEntries = file.Count;
                    int currentEntry = 0;

                    foreach (ZipEntry zipEntry in file)
                    {
                        currentEntry++;

                        if (progress != null)
                        {
                            progress.Report((double)currentEntry / totalEntries * 100);
                        }

                        if (!zipEntry.IsFile)
                        {
                            // Ignore directories
                            continue;
                        }

                        string entryFileName = zipEntry.Name;
                        // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                        // Optionally match entrynames against a selection list here to skip as desired.
                        // The unpacked length is available in the zipEntry.Size property.

                        // 4K is optimum
                        byte[] buffer = new byte[4096];
                        Stream zipStream = file.GetInputStream(zipEntry);

                        // Manipulate the output filename here as desired.
                        string fullZipToPath = Path.Combine(destinationFolder, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);

                        if (directoryName.Length > 0)
                        {
                            Directory.CreateDirectory(directoryName);
                        }

                        // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                        // of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
                finally
                {
                    if (file != null)
                    {
                        file.IsStreamOwner = true; // Makes close also shut the underlying stream
                        file.Close(); // Ensure we release resources
                    }
                }
            }

            public static bool CheckFileExist(string zipFilePath, string fileName, bool caseSensitive)
            {
                ZipFile file = null;
                try
                {
                    FileStream fs = File.OpenRead(zipFilePath);
                    file = new ZipFile(fs);

                    foreach (ZipEntry zipEntry in file)
                    {
                        if (caseSensitive)
                        {
                            if (zipEntry.Name == fileName)
                                return true;
                        }
                        else
                        {
                            if (zipEntry.Name.ToLower() == fileName.ToLower())
                                return true;
                        }
                    }
                }
                finally
                {
                    if (file != null)
                    {
                        file.IsStreamOwner = true; // Makes close also shut the underlying stream
                        file.Close(); // Ensure we release resources
                    }
                }
                return false;
            }

            public static async Task<string> ReadTextFromZip(string zipFilePath, string fileName)
            {
                ZipFile file = null;
                try
                {
                    FileStream fs = File.OpenRead(zipFilePath);
                    file = new ZipFile(fs);

                    foreach (ZipEntry zipEntry in file)
                    {
                        if (zipEntry.Name.ToLower() == fileName.ToLower())
                        {
                            Stream zipStream = file.GetInputStream(zipEntry);
                            using (StreamReader reader = new StreamReader(zipStream))
                                return await reader.ReadToEndAsync();
                        }
                    }
                }
                finally
                {
                    if (file != null)
                    {
                        file.IsStreamOwner = true; // Makes close also shut the underlying stream
                        file.Close(); // Ensure we release resources
                    }
                }
                return null;
            }
        }

        public static class Json
        {
            /*public static void ObjectToJsonFile(object obj, string destinationFile)
            {
                File.WriteAllText(destinationFile, JsonConvert.SerializeObject(obj, Formatting.Indented));
            }*/

            public static object DeserializeObject(string jsonStr)
            {
                return JsonConvert.DeserializeObject(jsonStr);
            }
        }
    }
}
