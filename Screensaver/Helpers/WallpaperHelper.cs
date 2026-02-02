using System.Runtime.InteropServices;
using System.Text;

namespace Screensaver.Helpers
{
    public class WallpaperHelper
    {
        private const int SPI_GETDESKWALLPAPER = 0x0073;
        private const int MAX_PATH = 260;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(
            int uAction,
            int uParam,
            StringBuilder lpvParam,
            int fuWinIni);

        public static string GetWallpaperPath()
        {
            var sb = new StringBuilder(MAX_PATH);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, sb.Capacity, sb, 0);
            return sb.ToString();
        }
    }
}
