using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Screensaver
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public uint AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private uint _blurOpacity = 128;
        public double BlurOpacity
        {
            get { return _blurOpacity; }
            set { _blurOpacity = (uint)value; EnableBlur(); }
        }

        private uint _blurBackgroundColor = 0x990000; /* BGR color format */

        public MainWindow()
        {
            InitializeComponent();
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            accent.GradientColor = (_blurOpacity << 24) | (_blurBackgroundColor & 0xFFFFFF);

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();

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

        private void Window_KeyDown(object sender, KeyEventArgs e) => this.Close();

        private void Window_Closed(object sender, EventArgs e)
        {
            HidePanelAnimation(sender, e);
        }

        private void ShowPanelAnimation(object? sender, EventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = Panel.ActualWidth,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            PanelTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void HidePanelAnimation(object? sender, EventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = Panel.ActualWidth,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
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