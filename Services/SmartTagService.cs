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
                // Original tags
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
                    "Analyze the provided text along with the screen context from the screenshot. Provide relevant assistance based on what you can see on the screen."),

                // Productivity & Writing (8 tags)
                new SmartTag("TL;DR", "/tldr", "Ultra-short summary (one sentence max)", 
                    "Create an extremely brief summary of the following text in one sentence maximum. Capture only the most essential point."),
                
                new SmartTag("Explain Like I'm 5", "/eli5", "Explain like I'm 5 (simple explanations)", 
                    "Explain the following text in the simplest possible terms, as if explaining to a 5-year-old. Use basic language and simple analogies."),
                
                new SmartTag("Deep Proofread", "/proofread", "Deep proofreading with suggestions", 
                    "Perform a thorough proofread of the following text. Identify and fix grammar, spelling, punctuation, and style issues. Provide the corrected version with brief explanations of major changes."),
                
                new SmartTag("Extract Keywords", "/keywords", "Extract key terms and phrases", 
                    "Extract the most important keywords, key phrases, and key terms from the following text. List them in order of importance."),
                
                new SmartTag("Create Outline", "/outline", "Convert to structured outline", 
                    "Convert the following text into a well-structured outline with main points and subpoints. Use proper numbering and indentation."),
                
                new SmartTag("Extract Actions", "/action", "Extract action items and tasks", 
                    "Extract all action items, tasks, and to-do items from the following text. List them as clear, actionable items."),
                
                new SmartTag("Meeting Notes", "/meeting", "Format as meeting notes with agenda", 
                    "Format the following text as professional meeting notes with agenda items, discussion points, decisions made, and action items."),
                
                new SmartTag("SEO Optimize", "/seo", "Optimize text for SEO with keywords", 
                    "Optimize the following text for search engines by improving keyword density, readability, and SEO best practices while maintaining natural flow."),

                // Communication & Tone (6 tags)
                new SmartTag("Make Confident", "/confident", "Make text more assertive and confident", 
                    "Rewrite the following text to be more assertive, confident, and decisive. Use strong, positive language while maintaining professionalism."),
                
                new SmartTag("Add Empathy", "/empathy", "Add empathetic and understanding tone", 
                    "Rewrite the following text to be more empathetic, understanding, and compassionate. Show genuine care and consideration for the reader's feelings."),
                
                new SmartTag("Make Persuasive", "/persuasive", "Make text more convincing", 
                    "Rewrite the following text to be more persuasive and convincing. Use compelling arguments, emotional appeals, and strong calls to action."),
                
                new SmartTag("Format Apology", "/apology", "Format as sincere apology", 
                    "Format the following text as a sincere, heartfelt apology. Include acknowledgment of the issue, genuine remorse, and commitment to improvement."),
                
                new SmartTag("Congratulate", "/congratulate", "Format as congratulations message", 
                    "Format the following text as a warm, genuine congratulations message. Express enthusiasm and recognition for the achievement."),
                
                new SmartTag("Politely Decline", "/decline", "Politely decline or say no", 
                    "Rewrite the following text as a polite, respectful decline. Be firm but courteous, and offer alternatives when appropriate."),

                // Coding & Technical (5 tags)
                new SmartTag("Debug Code", "/debug", "Analyze code for bugs and issues", 
                    "Analyze the following code for bugs, errors, and potential issues. Identify problems and suggest fixes with explanations."),
                
                new SmartTag("Add Comments", "/comment", "Add comprehensive code comments", 
                    "Add comprehensive, clear comments to the following code. Explain what each section does, why it's needed, and how it works."),
                
                new SmartTag("Optimize Code", "/optimize", "Suggest performance optimizations", 
                    "Analyze the following code for performance optimization opportunities. Suggest specific improvements for speed, memory usage, and efficiency."),
                
                new SmartTag("Generate Tests", "/test", "Generate unit tests for code", 
                    "Generate comprehensive unit tests for the following code. Include edge cases, error conditions, and normal operation scenarios."),
                
                new SmartTag("Explain Code", "/explain", "Explain code in plain English", 
                    "Explain the following code in plain English. Describe what it does, how it works, and what each part accomplishes."),

                // Creative & Fun (7 tags)
                new SmartTag("Make Funny", "/joke", "Make text humorous and funny", 
                    "Rewrite the following text to be humorous and funny while maintaining the original message. Add wit, humor, and entertainment value."),
                
                new SmartTag("Convert to Poem", "/poem", "Convert to poem or verse", 
                    "Convert the following text into a poem or verse. Use appropriate rhythm, rhyme, and poetic structure while preserving the meaning."),
                
                new SmartTag("Create Story", "/story", "Expand into short story", 
                    "Expand the following text into an engaging short story. Add characters, setting, plot development, and narrative elements."),
                
                new SmartTag("Add Emojis", "/emoji", "Add relevant emojis throughout", 
                    "Add appropriate and relevant emojis throughout the following text to enhance expression and engagement. Use emojis sparingly and meaningfully."),
                
                new SmartTag("Gen-Z Style", "/gen-z", "Rewrite in Gen-Z slang", 
                    "Rewrite the following text using Gen-Z slang, expressions, and modern internet language while keeping the original meaning."),
                
                new SmartTag("Pirate Speak", "/pirate", "Rewrite in pirate speak (fun!)", 
                    "Rewrite the following text in pirate speak using 'arr', 'matey', 'ye', and other pirate expressions. Keep it fun and entertaining."),
                
                new SmartTag("Shakespearean", "/shakespeare", "Rewrite in Shakespearean English", 
                    "Rewrite the following text in Shakespearean English using 'thou', 'thee', 'thy', and other Elizabethan expressions while maintaining the meaning."),

                // Data & Analysis (4 tags)
                new SmartTag("Compare Items", "/compare", "Compare and contrast items", 
                    "Compare and contrast the items mentioned in the following text. Highlight similarities, differences, advantages, and disadvantages."),
                
                new SmartTag("Pros and Cons", "/pros-cons", "List pros and cons", 
                    "Extract and list the pros and cons from the following text. Organize them clearly with advantages and disadvantages."),
                
                new SmartTag("Fact Check", "/fact-check", "Verify claims and provide sources", 
                    "Fact-check the claims in the following text. Identify any questionable statements and suggest reliable sources for verification."),
                
                new SmartTag("Extract Statistics", "/stats", "Extract numbers and statistics", 
                    "Extract all numbers, statistics, percentages, and quantitative data from the following text. Organize them clearly with context."),

                // Advanced Daily Use & Productivity (10 tags)
                new SmartTag("Create Schedule", "/schedule", "Extract dates, times, and create schedule format", 
                    "Extract all dates, times, and scheduling information from the following text. Format as a clear, organized schedule or timeline."),
                
                new SmartTag("Price Analysis", "/price", "Extract pricing info and calculate totals/comparisons", 
                    "Extract pricing information from the following text. Calculate totals, comparisons, and provide price analysis with breakdowns."),
                
                new SmartTag("Recipe Format", "/recipe", "Format as step-by-step recipe with ingredients", 
                    "Format the following text as a clear, step-by-step recipe with ingredients list, measurements, and cooking instructions."),
                
                new SmartTag("Step-by-Step Directions", "/directions", "Convert to clear step-by-step directions", 
                    "Convert the following text into clear, numbered step-by-step directions that are easy to follow and understand."),
                
                new SmartTag("Simplify Text", "/simplify", "Simplify complex text to basic language", 
                    "Simplify the following text to use basic, easy-to-understand language. Remove jargon and complex terms while keeping the meaning clear."),
                
                new SmartTag("Technical Language", "/technical", "Convert to technical/expert language", 
                    "Convert the following text to use technical, professional language appropriate for experts in the field. Use precise terminology and industry standards."),
                
                new SmartTag("LinkedIn Format", "/linkedin", "Optimize for LinkedIn post format", 
                    "Optimize the following text for LinkedIn by making it professional, engaging, and suitable for business networking. Include relevant hashtags."),
                
                new SmartTag("Twitter Format", "/tweet", "Condense to Twitter/X format (280 chars)", 
                    "Condense the following text to fit Twitter/X's 280-character limit while maintaining the key message and impact."),
                
                new SmartTag("Generate Hashtags", "/hashtags", "Generate relevant hashtags for social media", 
                    "Generate relevant, trending hashtags for the following text that would be appropriate for social media platforms."),
                
                new SmartTag("Generate Reply", "/reply", "Generate appropriate reply to message/email", 
                    "Generate an appropriate, professional reply to the following message or email. Match the tone and address all points mentioned."),

                // Smart Processing & Context (8 tags)
                new SmartTag("Continue Text", "/continue", "Continue/complete the text naturally", 
                    "Continue and complete the following text naturally. Maintain the same style, tone, and direction while adding meaningful content."),
                
                new SmartTag("Rephrase Text", "/rephrase", "Rephrase without changing meaning", 
                    "Rephrase the following text using different words and sentence structures while keeping the exact same meaning and message."),
                
                new SmartTag("Generate Questions", "/questions", "Generate relevant questions about the topic", 
                    "Generate thoughtful, relevant questions about the topic discussed in the following text. Include both basic and advanced questions."),
                
                new SmartTag("Create Title", "/title", "Generate catchy title/headline", 
                    "Generate a catchy, engaging title or headline for the following text that would grab attention and accurately represent the content."),
                
                new SmartTag("Generate Tags", "/tags", "Generate categorization tags", 
                    "Generate relevant categorization tags for the following text that would help with organization, search, and content classification."),
                
                new SmartTag("Sentiment Analysis", "/sentiment", "Analyze sentiment (positive/negative/neutral)", 
                    "Analyze the sentiment of the following text and determine if it's positive, negative, or neutral. Provide a brief explanation of the analysis."),
                
                new SmartTag("Improve Readability", "/readability", "Improve readability and clarity", 
                    "Improve the readability and clarity of the following text by simplifying sentence structure, improving flow, and enhancing comprehension."),
                
                new SmartTag("Legal Format", "/legal", "Convert to legal/formal document language", 
                    "Convert the following text to formal legal document language using appropriate legal terminology, structure, and formal tone.")
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
                
                // Debug: Log the prompt being sent
                System.Diagnostics.Debug.WriteLine($"Smart Tag Prompt: {fullPrompt}");
                
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
                
                // Debug: Log the result to see what we're getting
                System.Diagnostics.Debug.WriteLine($"Smart Tag Result: {result}");
                
                // Check if result is garbled or corrupted
                if (string.IsNullOrWhiteSpace(result) || result.Length < 10)
                {
                    return request.CleanText; // Return original if no valid result
                }
                
                // Check for garbled text (contains too many random characters)
                var garbledThreshold = 0.3; // 30% of characters should be letters/spaces
                var letterCount = result.Count(c => char.IsLetter(c) || char.IsWhiteSpace(c));
                var garbledRatio = (double)letterCount / result.Length;
                
                if (garbledRatio < garbledThreshold)
                {
                    System.Diagnostics.Debug.WriteLine($"Garbled text detected: {result}");
                    return request.CleanText; // Return original if garbled
                }
                
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
        
        // Test method to verify smart tag functionality
        public async Task<string> TestSmartTag(string trigger, string testText)
        {
            var tag = _availableTags.FirstOrDefault(t => t.Trigger == trigger);
            if (tag == null) return "Tag not found";
            
            var request = new SmartTagRequest(testText, testText, tag);
            return await ProcessSmartTagAsync(request);
        }
    }
}