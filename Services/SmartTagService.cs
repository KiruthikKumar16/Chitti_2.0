using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using LineBuddy.Models;
using LineBuddy.Services;

namespace LineBuddy.Services
{
    public class SmartTagService
    {
        private readonly LLMService _llmService;
        private readonly ConversationHistoryService _historyService;
        private readonly ScreenshotService _screenshotService;
        private List<SmartTag> _availableTags;
        private bool _shareContextWithChat;
        
        public event Action<SmartTagStatus> StatusChanged;
        public event Action<string> ProcessingUpdate;
        
        public SmartTagService(LLMService llmService, ConversationHistoryService historyService, ScreenshotService screenshotService = null)
        {
            _llmService = llmService;
            _historyService = historyService;
            _screenshotService = screenshotService ?? new ScreenshotService();
            InitializeDefaultTags();
        }
        
        public void UpdateSettings(bool shareContext, List<SmartTag> customTags = null)
        {
            _shareContextWithChat = shareContext;
            if (customTags != null)
            {
                // Remove old custom tags and add new ones
                _availableTags.RemoveAll(t => t.IsCustom);
                _availableTags.AddRange(customTags);
            }
        }
        
        private void InitializeDefaultTags()
        {
            _availableTags = new List<SmartTag>
            {
                new SmartTag("Grammar Fix", "/grammar", "Fix grammar and spelling errors", 
                    "Fix the grammar, spelling, and punctuation in the following text. Return only the corrected text with no additional commentary."),
                
                new SmartTag("Make Polite", "/polite", "Make text more polite and professional", 
                    "Rewrite the following text to be more polite, professional, and courteous. Maintain the original meaning but improve the tone."),
                
                new SmartTag("Make Formal", "/formal", "Convert to formal language", 
                    "Rewrite the following text in a formal, professional tone suitable for business communication. Maintain the original meaning."),
                
                new SmartTag("Make Casual", "/casual", "Convert to casual language", 
                    "Rewrite the following text in a casual, friendly tone suitable for informal communication. Maintain the original meaning."),
                
                new SmartTag("Summarize", "/summary", "Create a concise summary", 
                    "Create a concise summary of the following text. Capture the main points in a brief, clear manner."),
                
                new SmartTag("Expand", "/expand", "Expand and elaborate on text", 
                    "Expand and elaborate on the following text. Add more detail, context, and explanation while maintaining the original meaning."),
                
                new SmartTag("Translate", "/translate", "Translate to English", 
                    "Translate the following text to English. If it's already in English, identify the language and translate to the most appropriate target language."),
                
                new SmartTag("Solve Problem", "/solve", "Provide solution or answer", 
                    "Analyze the following text and provide a clear, helpful solution or answer. Be practical and actionable in your response."),
                
                new SmartTag("Code Review", "/code", "Review and improve code", 
                    "Review the following code and suggest improvements. Focus on best practices, efficiency, and readability. Provide the improved version."),
                
                new SmartTag("Make Creative", "/creative", "Add creativity and flair", 
                    "Rewrite the following text to be more creative, engaging, and interesting while maintaining the original message."),
                
                new SmartTag("Bullet Points", "/bullets", "Convert to bullet points", 
                    "Convert the following text into clear, concise bullet points. Organize the information logically."),
                
                new SmartTag("Email Format", "/email", "Format as professional email", 
                    "Format the following text as a professional email with appropriate subject line, greeting, body, and closing."),
                
                new SmartTag("Screen Context", "/screen", "Process with screen context", 
                    "Analyze the provided text along with the screen context from the screenshot. Provide relevant assistance based on what you can see on the screen.")
            };
        }
        
        public bool HasSmartTag(string text)
        {
            return _availableTags.Any(tag => tag.IsEnabled && text.Contains(tag.Trigger));
        }
        
        public SmartTagRequest ParseSmartTag(string text)
        {
            var enabledTags = _availableTags.Where(t => t.IsEnabled).ToList();
            
            foreach (var tag in enabledTags)
            {
                if (text.Contains(tag.Trigger))
                {
                    // Remove the tag from the text
                    string cleanText = text.Replace(tag.Trigger, "").Trim();
                    return new SmartTagRequest(text, cleanText, tag);
                }
            }
            
            return null;
        }
        
        public async Task<string> ProcessSmartTagAsync(SmartTagRequest request)
        {
            try
            {
                StatusChanged?.Invoke(SmartTagStatus.Processing);
                ProcessingUpdate?.Invoke($"Processing with {request.Tag.Name}...");
                
                // Build context if sharing with chat
                string context = "";
                if (_shareContextWithChat)
                {
                    var history = _historyService.GetContextualHistory();
                    if (!string.IsNullOrEmpty(history))
                    {
                        context = $"Previous conversation context:\n{history}\n\nNow process this text:\n";
                    }
                }
                
                // Create the full prompt
                string fullPrompt = $"{request.Tag.SystemPrompt}\n\n{context}{request.CleanText}";
                
                // Capture screenshot if this is a /screen tag
                string screenshotData = "";
                if (request.Tag.Trigger == "/screen")
                {
                    try
                    {
                        screenshotData = await _screenshotService.CaptureActiveWindowAsync();
                        ProcessingUpdate?.Invoke("Processing with screen context...");
                    }
                    catch (Exception ex)
                    {
                        ProcessingUpdate?.Invoke($"Screenshot failed, processing text only: {ex.Message}");
                    }
                }
                
                // Process with LLM (with or without screenshot)
                var result = await _llmService.QueryAsync(fullPrompt, "", screenshotData);
                
                StatusChanged?.Invoke(SmartTagStatus.Pasting);
                ProcessingUpdate?.Invoke("Pasting result...");
                
                return result;
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(SmartTagStatus.Error);
                ProcessingUpdate?.Invoke($"Error: {ex.Message}");
                return request.CleanText; // Return original on error
            }
        }
        
        public List<SmartTag> GetAvailableTags()
        {
            return _availableTags.ToList();
        }
        
        public void AddCustomTag(SmartTag customTag)
        {
            customTag.IsCustom = true;
            _availableTags.Add(customTag);
        }
        
        public void RemoveCustomTag(string trigger)
        {
            _availableTags.RemoveAll(t => t.IsCustom && t.Trigger == trigger);
        }
        
        public void UpdateTagStatus(string trigger, bool enabled)
        {
            var tag = _availableTags.FirstOrDefault(t => t.Trigger == trigger);
            if (tag != null)
            {
                tag.IsEnabled = enabled;
            }
        }
        
        public void SetListening()
        {
            StatusChanged?.Invoke(SmartTagStatus.Listening);
            ProcessingUpdate?.Invoke("Listening for Smart Tags...");
        }
    }
}