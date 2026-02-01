using Models;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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
using static System.Net.Mime.MediaTypeNames;

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


    [StructLayout(LayoutKind.Sequential)]
    struct XInputState
    {
        public uint dwPacketNumber;
        public XInputGamepad Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct XInputGamepad
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("xinput1_4.dll")]
        static extern int XInputGetState(int index, out XInputState state);

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
            SetupScreensaver();

            var culture = CultureInfo.CurrentUICulture;

            string idioma = culture.TwoLetterISOLanguageName; // "es", "en"
            string cultura = culture.Name;
        }

        private void SetupScreensaver()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
            Topmost = true;
            ShowInTaskbar = false;
            Cursor = Cursors.None;
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

            RefreshHintAndTipView();
            GamepadHandler();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) => this.Close();

        private void Window_Closed(object sender, EventArgs e) => HidePanelAnimation(sender, e);

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

        private void RefreshTipAndHintAnimation(HintAndTip hint)
        {
            TitleText.Text = hint.Title;
            BodyText.Text = hint.Content;

            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream($"Screensaver.Media.{hint.Image}");

            if (stream != null)
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = stream;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();

                HintImage.Source = img;
            }

            var swipeInAnimation = new DoubleAnimation
            {
                From = (TitleText.ActualWidth / 4) * 3,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(450),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            TitleTransform.BeginAnimation(TranslateTransform.XProperty, swipeInAnimation);
            BodyTransform.BeginAnimation(TranslateTransform.XProperty, swipeInAnimation);
            HintImageTransform.BeginAnimation(TranslateTransform.XProperty, swipeInAnimation);
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

        private void RefreshHintAndTipView()
        {
            TitleText.Text = "";
            BodyText.Text = "";

            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("Screensaver.Data.TipsStrings.json");

            if (stream == null)
                return;

            using var sreader = new StreamReader(stream);
            var json = sreader.ReadToEnd();
            var hints = JsonSerializer.Deserialize<List<HintAndTip>>(json);

            if (hints == null)
                return;

            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(25) };

            TitleText.Text = hints[0].Title;
            BodyText.Text = hints[0].Content;

            using var imgstream = asm.GetManifestResourceStream($"Screensaver.Media.{hints[0].Image}");

            if (stream != null)
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = imgstream;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();

                HintImage.Source = img;
            }

            timer.Tick += (s, e) =>
            {
                var i = new Random().Next(0, hints.Count);

                RefreshTipAndHintAnimation(hints[i]);
            };
            timer.Start();
        }

        private void GamepadHandler()
        {
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };

            timer.Tick += (s, e) =>
            {
                if (XInputGetState(0, out var state) == 0)
                {
                    if (state.Gamepad.wButtons != 0 ||
                        state.Gamepad.sThumbLX != 0 ||
                        state.Gamepad.sThumbLY != 0 ||
                        state.Gamepad.bLeftTrigger != 0 ||
                        state.Gamepad.bRightTrigger != 0)
                    {
                        Close();
                    }
                }
            };

            timer.Start();
        }
    }
}