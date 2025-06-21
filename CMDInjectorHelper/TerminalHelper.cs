using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDInjectorHelper
{
    public static class TerminalHelper
    {
        public static int FontSize
        {
            get
            {
                var fontSize = Helper.LocalSettingsHelper.LoadSettings("ConFontSizeSet", 3);
                if (fontSize == 0) return 15;
                else if (fontSize == 1) return 16;
                else if (fontSize == 2) return 17;
                else if (fontSize == 3) return 18;
                else if (fontSize == 4) return 19;
                else if (fontSize == 5) return 20;
                return 17;
            }
        }

        public static string DirectoryFile
        {
            get { return $"{Helper.localFolder.Path}\\TerminalDirectory.txt"; }
        }

        public static string ResultFile
        {
            get { return $"{Helper.localFolder.Path}\\TerminalResult.txt"; }
        }

        public static string EndFile
        {
            get { return $"{Helper.localFolder.Path}\\TerminalEnd.txt"; }
        }
    }
}
