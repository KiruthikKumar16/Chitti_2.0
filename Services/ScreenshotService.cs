using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace LineBuddy.Services
{
    public class ScreenshotService
    {
        public async Task<string> CaptureScreenshotAsync()
        {
            try
            {
                // Get the bounds of all screens
                Rectangle bounds = SystemInformation.VirtualScreen;
                
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        // Capture the screen
                        graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
                    }
                    
                    // Convert to base64 string
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Jpeg);
                        byte[] imageBytes = ms.ToArray();
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to capture screenshot: {ex.Message}");
            }
        }
        
        public async Task<string> CaptureActiveWindowAsync()
        {
            try
            {
                // First try to get the foreground window (the window that was active before Smart Bar)
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    // Fallback to full screen if no foreground window
                    return await CaptureScreenshotAsync();
                }
                
                // Get window title to check if it's Smart Bar
                string windowTitle = GetWindowTitle(foregroundWindow);
                if (windowTitle.Contains("Smart Bar") || windowTitle.Contains("LineBuddy"))
                {
                    // If Smart Bar is the active window, capture full screen instead
                    return await CaptureScreenshotAsync();
                }
                
                // Get window bounds
                if (!GetWindowRect(foregroundWindow, out RECT rect))
                {
                    // Fallback to full screen if can't get window bounds
                    return await CaptureScreenshotAsync();
                }
                
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                
                // If window is too small (like Smart Bar), capture full screen instead
                if (width < 200 || height < 100)
                {
                    return await CaptureScreenshotAsync();
                }
                
                using (Bitmap bitmap = new Bitmap(width, height))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        // Capture the window
                        graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                    }
                    
                    // Convert to base64 string
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Jpeg);
                        byte[] imageBytes = ms.ToArray();
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to capture active window: {ex.Message}");
            }
        }
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
        
        private string GetWindowTitle(IntPtr hwnd)
        {
            var title = new System.Text.StringBuilder(256);
            GetWindowText(hwnd, title, title.Capacity);
            return title.ToString();
        }
        
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}