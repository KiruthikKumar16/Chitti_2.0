using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LineBuddy.Models
{
    public class SmartTag
    {
        public string Name { get; set; }
        public string Trigger { get; set; }
        public string Description { get; set; }
        public string SystemPrompt { get; set; }
        public bool IsCustom { get; set; }
        public bool IsEnabled { get; set; } = true;
        
        public SmartTag() { }
        
        public SmartTag(string name, string trigger, string description, string systemPrompt, bool isCustom = false)
        {
            Name = name;
            Trigger = trigger;
            Description = description;
            SystemPrompt = systemPrompt;
            IsCustom = isCustom;
        }
    }

    public class SmartTagRequest
    {
        public string OriginalText { get; set; }
        public string CleanText { get; set; }
        public SmartTag Tag { get; set; }
        public DateTime Timestamp { get; set; }
        
        public SmartTagRequest(string originalText, string cleanText, SmartTag tag)
        {
            OriginalText = originalText;
            CleanText = cleanText;
            Tag = tag;
            Timestamp = DateTime.Now;
        }
    }

    public enum SmartTagStatus
    {
        Listening,
        Processing,
        Pasting,
        Error,
        Disabled
    }
    
    public class SmartTagProcessor
    {
        public bool IsListening { get; set; } = true;
        public TimeSpan MonitorDelay { get; set; } = TimeSpan.FromMilliseconds(500);
    }
    
    public class SmartTagSettings
    {
        public bool IsEnabled { get; set; } = true;
        public bool ShareContextWithChat { get; set; } = true;
        public bool AutoPasteResults { get; set; } = true;
        public List<SmartTag> CustomTags { get; set; } = new List<SmartTag>();
    }
    
    public class ClipboardMonitorService
    {
        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        
        [DllImport("user32.dll")]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        
        public bool StartMonitoring(IntPtr windowHandle)
        {
            return AddClipboardFormatListener(windowHandle);
        }
        
        public bool StopMonitoring(IntPtr windowHandle)
        {
            return RemoveClipboardFormatListener(windowHandle);
        }
    }
}