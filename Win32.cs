using System;
using System.Runtime.InteropServices;

namespace LineBuddy
{
    internal static class Win32
    {
        [DllImport("shell32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public int lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public const int ABM_NEW = 0x00000000;
        public const int ABM_REMOVE = 0x00000001;
        public const int ABM_QUERYPOS = 0x00000002;
        public const int ABM_SETPOS = 0x00000003;
        public const int ABM_GETSTATE = 0x00000004;
        public const int ABM_GETTASKBARPOS = 0x00000005;
        public const int ABM_ACTIVATE = 0x00000006;
        public const int ABM_GETAUTOHIDEBAR = 0x00000007;
        public const int ABM_SETAUTOHIDEBAR = 0x00000008;
        public const int ABM_WINDOWPOSCHANGED = 0x00000009;
        public const int ABM_SETSTATE = 0x0000000A;

        public const int ABE_LEFT = 0;
        public const int ABE_TOP = 1;
        public const int ABE_RIGHT = 2;
        public const int ABE_BOTTOM = 3;

        public const int WM_USER = 0x0400;

        // SetWindowPos constants
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_SHOWWINDOW = 0x0040;
    }
}