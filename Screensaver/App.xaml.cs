using System.Configuration;
using System.Data;
using System.Windows;

namespace Screensaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void RunSaver()
        {
            var saver = new MainWindow();
            //this.MainWindow = saver;

            saver.Show();
            //saver.Closing += Saver_Closing;
        }

        private void LaunchConfig()
        {
            MessageBox.Show("This screensaver has no options that you can set yet.", "No options", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Saver_Closing(object? sender, EventArgs e)
        {
            this.Shutdown();
        }

        private void Preview(string[] args)
        {
            if (args.Length < 2)
                return;

            if (!IntPtr.TryParse(args[1], out var previewHandle))
                return;

            var previewWindow = new PreviewWindow(previewHandle);
            previewWindow.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                RunSaver();

                //ShowConfig();
                return;
            }

            switch (e.Args[0].ToLower())
            {
                case "/s":
                    RunSaver();
                    break;

                case "/p":
                    Preview(e.Args);
                    break;

                case "/c":
                    LaunchConfig();
                    break;

                default:
                    //ShowConfig();
                    break;
            }
        }
    }
}
