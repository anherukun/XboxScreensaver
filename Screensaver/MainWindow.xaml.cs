using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Screensaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Panel.RenderTransform.Transform(new System.Windows.Point(Panel.ActualWidth, 0));
            PanelTransform.X = Panel.ActualWidth;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 0.6,
                Duration = TimeSpan.FromSeconds(0.3),
            };
            animation.Completed += ShowPanelAnimation;

            Background.BeginAnimation(OpacityProperty, animation);
        }

        private void ShowPanelAnimation(object? sender, EventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = Panel.ActualWidth,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(750),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            PanelTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void HidePanelAnimation(object? sender, EventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = Panel.ActualWidth,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500)
            };

            Panel.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void Clock_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            TextBlock textBlock = sender as TextBlock;
            DispatcherTimer timer = new DispatcherTimer();

            textBlock.Text = $"{DateTime.Now.ToString("hh:mm tt")}";

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (s, e) =>
            {
                textBlock.Text = $"{DateTime.Now.ToString("hh:mm tt")}";
            };
            timer.Start();

        }
    }
}