using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Screensaver
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        private readonly IntPtr _parent;

        public PreviewWindow(IntPtr parent)
        {
            InitializeComponent();
            _parent = parent;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;

            SetParent(hwnd, _parent);

            SetWindowLong(hwnd, GWL_STYLE,
                GetWindowLong(hwnd, GWL_STYLE) | WS_CHILD);

            ResizeToParent();
        }

        private void ResizeToParent()
        {
            GetClientRect(_parent, out RECT rect);
            MoveWindow(
                new WindowInteropHelper(this).Handle,
                0, 0,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top,
                true);
        }

        #region Win32

        private const int GWL_STYLE = -16;
        private const int WS_CHILD = 0x40000000;

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(
            IntPtr hWnd,
            int X, int Y,
            int nWidth, int nHeight,
            bool bRepaint);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        #endregion
    }
}
