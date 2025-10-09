using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace LineBuddy
{
    public class AppBarManager
    {
        private IntPtr _appBarWindow;
        private Window _wpfWindow;
        private const int WM_APPBAR_CALLBACK = 0x0400 + 1000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const uint WS_POPUP = 0x80000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public void RegisterAppBar(Window window)
        {
            _wpfWindow = window;
            
            // Create a hidden Win32 window for AppBar registration
            var hInstance = GetModuleHandle(null);
            _appBarWindow = CreateWindowEx(
                0, "STATIC", "LineBuddyAppBar", WS_POPUP,
                0, 0, (int)SystemParameters.PrimaryScreenWidth, (int)window.Height,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (_appBarWindow != IntPtr.Zero)
            {
                // Hide the Win32 window (we only need it for AppBar registration)
                ShowWindow(_appBarWindow, SW_HIDE);
                
                // Register as AppBar
                var abd = new Win32.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = _appBarWindow;
                abd.uCallbackMessage = WM_APPBAR_CALLBACK;

                // Register the appbar
                Win32.SHAppBarMessage(Win32.ABM_NEW, ref abd);

                // Set position to top
                abd.uEdge = Win32.ABE_TOP;
                abd.rc.left = 0;
                abd.rc.top = 0;
                abd.rc.right = (int)SystemParameters.PrimaryScreenWidth;
                abd.rc.bottom = (int)window.Height;

                Win32.SHAppBarMessage(Win32.ABM_QUERYPOS, ref abd);
                Win32.SHAppBarMessage(Win32.ABM_SETPOS, ref abd);

                // Now position our WPF window at the absolute top
                var windowHelper = new WindowInteropHelper(window);
                Win32.SetWindowPos(windowHelper.Handle, Win32.HWND_TOPMOST, 0, 0, 
                    (int)SystemParameters.PrimaryScreenWidth, (int)window.Height, 
                    Win32.SWP_SHOWWINDOW);
                
                // Force WPF window to stay at top
                window.Left = 0;
                window.Top = 0;
                window.Width = SystemParameters.PrimaryScreenWidth;
                window.Topmost = true;
            }
        }

        public void UnregisterAppBar()
        {
            if (_appBarWindow != IntPtr.Zero)
            {
                var abd = new Win32.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = _appBarWindow;
                Win32.SHAppBarMessage(Win32.ABM_REMOVE, ref abd);

                DestroyWindow(_appBarWindow);
                _appBarWindow = IntPtr.Zero;
            }
        }
    }
}