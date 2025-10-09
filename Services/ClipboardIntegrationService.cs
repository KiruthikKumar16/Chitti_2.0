using System;
using System.Windows;
using System.Windows.Interop;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using LineBuddy.Models;
using LineBuddy.Services;
using System.Windows.Forms;

namespace LineBuddy.Services
{
    public class ClipboardIntegrationService
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        
        private readonly SmartTagService _smartTagService;
        private readonly ClipboardMonitorService _clipboardMonitor;
        private readonly Window _parentWindow;
        private readonly AppSettings _settings;
        private HwndSource _hwndSource;
        private bool _isMonitoring = false;
        private bool _isProcessing = false;
        
        public event Action<SmartTagStatus> StatusChanged;
        public event Action<string> ProcessingUpdate;
        
        public ClipboardIntegrationService(SmartTagService smartTagService, Window parentWindow, AppSettings settings)
        {
            _smartTagService = smartTagService;
            _parentWindow = parentWindow;
            _settings = settings;
            _clipboardMonitor = new ClipboardMonitorService();
            
            // Subscribe to smart tag service events
            _smartTagService.StatusChanged += OnSmartTagStatusChanged;
            _smartTagService.ProcessingUpdate += OnProcessingUpdate;
        }
        
        public void StartMonitoring()
        {
            if (_isMonitoring) return;
            
            try
            {
                // Get the window handle
                var windowInteropHelper = new WindowInteropHelper(_parentWindow);
                var hwnd = windowInteropHelper.Handle;
                
                if (hwnd == IntPtr.Zero)
                {
                    // Window not yet loaded, wait for it
                    _parentWindow.Loaded += (s, e) => StartMonitoring();
                    return;
                }
                
                // Create HwndSource for message handling
                _hwndSource = HwndSource.FromHwnd(hwnd);
                _hwndSource?.AddHook(WndProc);
                
                // Start clipboard monitoring
                if (_clipboardMonitor.StartMonitoring(hwnd))
                {
                    _isMonitoring = true;
                    _smartTagService.SetListening();
                    ProcessingUpdate?.Invoke("Smart Tags listening for clipboard changes...");
                }
                else
                {
                    ProcessingUpdate?.Invoke("Failed to start clipboard monitoring");
                }
            }
            catch (Exception ex)
            {
                ProcessingUpdate?.Invoke($"Error starting clipboard monitor: {ex.Message}");
            }
        }
        
        public void StopMonitoring()
        {
            if (!_isMonitoring) return;
            
            try
            {
                var windowInteropHelper = new WindowInteropHelper(_parentWindow);
                var hwnd = windowInteropHelper.Handle;
                
                if (hwnd != IntPtr.Zero)
                {
                    _clipboardMonitor.StopMonitoring(hwnd);
                }
                
                _hwndSource?.RemoveHook(WndProc);
                _isMonitoring = false;
                
                StatusChanged?.Invoke(SmartTagStatus.Disabled);
                ProcessingUpdate?.Invoke("Smart Tags monitoring stopped");
            }
            catch (Exception ex)
            {
                ProcessingUpdate?.Invoke($"Error stopping clipboard monitor: {ex.Message}");
            }
        }
        
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE && !_isProcessing)
            {
                // Handle clipboard change on background thread
                Task.Run(async () => await HandleClipboardChange());
            }
            
            return IntPtr.Zero;
        }
        
        private async Task HandleClipboardChange()
        {
            if (_isProcessing) return;
            
            try
            {
                _isProcessing = true;
                
                // Small delay to ensure clipboard is fully updated
                await Task.Delay(100);
                
                string clipboardText = null;
                
                // Get clipboard text on UI thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        if (System.Windows.Clipboard.ContainsText())
                        {
                            clipboardText = System.Windows.Clipboard.GetText();
                        }
                    }
                    catch (Exception ex)
                    {
                        ProcessingUpdate?.Invoke($"Error reading clipboard: {ex.Message}");
                    }
                });
                
                if (string.IsNullOrWhiteSpace(clipboardText))
                {
                    return;
                }
                
                // Check if clipboard contains smart tags
                if (_smartTagService.HasSmartTag(clipboardText))
                {
                    // Parse the smart tag
                    var request = _smartTagService.ParseSmartTag(clipboardText);
                    if (request != null)
                    {
                        // Process the smart tag
                        var result = await _smartTagService.ProcessSmartTagAsync(request);
                        
                        // Replace clipboard content with result on UI thread
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                StatusChanged?.Invoke(SmartTagStatus.Pasting);
                                ProcessingUpdate?.Invoke($"Pasting with {request.Tag.Name}...");
                                
                                // Use typing animation for pasting if enabled
                                if (_settings.SmartTagsPastingSpeed > 0)
                                {
                                    Task.Run(async () => await TypeTextAsync(result));
                                }
                                else
                                {
                                    // Instant pasting
                                    System.Windows.Clipboard.SetText(result);
                                    Task.Run(async () =>
                                    {
                                        await Task.Delay(200);
                                        SendKeys.SendWait("^v");
                                    });
                                }
                                
                                // Return to listening after a delay
                                Task.Run(async () =>
                                {
                                    await Task.Delay(1000);
                                    StatusChanged?.Invoke(SmartTagStatus.Listening);
                                    ProcessingUpdate?.Invoke("Smart Tags ready");
                                });
                            }
                            catch (Exception ex)
                            {
                                ProcessingUpdate?.Invoke($"Error pasting: {ex.Message}");
                                StatusChanged?.Invoke(SmartTagStatus.Error);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessingUpdate?.Invoke($"Error processing clipboard: {ex.Message}");
                StatusChanged?.Invoke(SmartTagStatus.Error);
            }
            finally
            {
                _isProcessing = false;
            }
        }
        
        private void OnSmartTagStatusChanged(SmartTagStatus status)
        {
            StatusChanged?.Invoke(status);
        }
        
        private void OnProcessingUpdate(string message)
        {
            ProcessingUpdate?.Invoke(message);
        }
        
        private async Task TypeTextAsync(string text)
        {
            try
            {
                // Type each character with delay
                foreach (char c in text)
                {
                    // Special handling for certain characters
                    string charToSend = c.ToString();
                    
                    // Handle special characters that SendKeys needs escape sequences for
                    switch (c)
                    {
                        case '+':
                            charToSend = "{+}";
                            break;
                        case '^':
                            charToSend = "{^}";
                            break;
                        case '%':
                            charToSend = "{%}";
                            break;
                        case '~':
                            charToSend = "{~}";
                            break;
                        case '(':
                            charToSend = "{(}";
                            break;
                        case ')':
                            charToSend = "{)}";
                            break;
                        case '[':
                            charToSend = "{[}";
                            break;
                        case ']':
                            charToSend = "{]}";
                            break;
                        case '{':
                            charToSend = "{{}";
                            break;
                        case '}':
                            charToSend = "{}}";
                            break;
                        case '\r':
                        case '\n':
                            // Skip newlines to avoid automatic Enter presses
                            continue;
                        case '\t':
                            charToSend = "{TAB}";
                            break;
                    }
                    
                    SendKeys.SendWait(charToSend);
                    
                    // Wait based on typing speed setting
                    if (_settings.SmartTagsPastingSpeed > 0)
                    {
                        await Task.Delay(_settings.SmartTagsPastingSpeed);
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to clipboard paste on error
                ProcessingUpdate?.Invoke($"Typing failed, using clipboard: {ex.Message}");
                System.Windows.Clipboard.SetText(text);
                await Task.Delay(100);
                SendKeys.SendWait("^v");
            }
        }
        
        public void Dispose()
        {
            StopMonitoring();
            _hwndSource?.RemoveHook(WndProc);
            
            if (_smartTagService != null)
            {
                _smartTagService.StatusChanged -= OnSmartTagStatusChanged;
                _smartTagService.ProcessingUpdate -= OnProcessingUpdate;
            }
        }
    }
}