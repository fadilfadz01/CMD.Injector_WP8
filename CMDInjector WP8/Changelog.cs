using CMDInjectorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel;

namespace CMDInjector_WP8
{
    class Changelog
    {
        static readonly string currentVersion = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        public static void DisplayLog()
        {
            Helper.DisplayMessage1($"CMD Injector WP8 v{currentVersion}\n" +
                " • App will now install the downloaded update itself.\n" +
                " • Added GUI for Bcdedit in BootConfig.\n" +
                " • Added tweaks in the TweakBox.\n" +
                " • Added CMD un-injection option.\n" +
                " • Added various options to thank me by buying me a coffee in the About menu.\n" +
                " • Able to set command as argument during Terminal tile pinning.\n" +
                " • Able to install App packages to SD card.\n" +
                " • Able to install boot App Developer Menu.\n" +
                " • Able to pin Power Options tile to start screen.\n" +
                " • Updated FAQ.\n" +
                " • Bug fixes.\n" +
                " • Many other improvements.\n\n\n" +
                "CMD Injector WP8 v1.1.0.0\n" +
                " • Added Startup.\n" +
                " • Added some console applications.\n" +
                " • Able to check the App update from About.\n" +
                " • Able to install, update or register Appx/Bundle packages from PacMan.\n" +
                " • Able to pin menu items tile to start screen.\n" +
                " • Bug fixes.\n" +
                " • Many other improvements.\n\n\n" +
                "CMD Injector WP8 v1.0.0.0\n" +
                " • Added CMD injection.\n" +
                " • Added Terminal.\n" +
                " • Added PacMan Installer.\n" +
                " • Many other improvements.\n\n\n", Helper.SoundHelper.Sound.Alert, "Changelog");
        }
    }
}
