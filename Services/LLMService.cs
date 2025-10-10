using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace LineBuddy.Services
{
    public class LLMService
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _settings;

        public LLMService(AppSettings settings)
        {
            _httpClient = new HttpClient();
            _settings = settings;
        }

        public async Task<string> QueryAsync(string query, string conversationContext = "", string screenshotBase64 = "")
        {
            try
            {
                // Validate API key
                if (string.IsNullOrEmpty(_settings.LLMApiKey))
                {
                    return "Please configure your LLM API key in Settings.";
                }

                // Combine context with current query if context is provided
                var fullQuery = string.IsNullOrEmpty(conversationContext) 
                    ? query 
                    : $"{conversationContext}{query}";

                // Load personality (cached per process via static)
                var personalityPrompt = PersonalityProvider.GetSystemPrompt();

                // Build the request based on provider
                var requestBody = BuildRequestBody(fullQuery, screenshotBase64, personalityPrompt);
                var apiUrl = BuildApiUrl();

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add authorization header based on provider
                AddAuthorizationHeader();

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Debug: Log the response
                System.Diagnostics.Debug.WriteLine($"LLM Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"LLM Response Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return $"API Error ({response.StatusCode}): {responseContent}";
                }

                var parsedResult = ParseResponse(responseContent);
                System.Diagnostics.Debug.WriteLine($"Parsed Result: {parsedResult}");
                return parsedResult;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Actions planner: returns a compact JSON string plan
        public async Task<string> QueryActionsAsync(string naturalInstruction)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.LLMApiKey))
                {
                    return string.Empty;
                }
                var system = PersonalityProvider.GetSystemPrompt() +
                    "\nYou can also plan actions. If the user intent is a command, return ONLY valid minified JSON with keys action,args,risk (safe|risky). If it's a question, return empty string.";
                var request = BuildOpenAIRequestForActions(naturalInstruction, system);
                var apiUrl = BuildApiUrl();

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) return string.Empty;
                return ParseActionsResponse(responseContent);
            }
            catch
            {
                return string.Empty;
            }
        }

        private object BuildOpenAIRequestForActions(string userText, string system)
        {
            // Works for OpenAI-compatible endpoints and Gemini via generic handler
            return new
            {
                model = _settings.LLMModel,
                messages = new object[]
                {
                    new { role = "system", content = system },
                    new { role = "user", content = userText },
                    new { role = "system", content = "Return ONLY compact JSON for commands, else empty string. Example: {\"action\":\"searchImages\",\"args\":{\"engine\":\"google\",\"query\":\"logo\",\"count\":5},\"risk\":\"safe\"}" }
                }
            };
        }

        private string ParseActionsResponse(string responseContent)
        {
            try
            {
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string content = jsonResponse?.choices?[0]?.message?.content;
                if (string.IsNullOrWhiteSpace(content)) return string.Empty;
                // Heuristic: ensure it looks like JSON plan
                if (content.Contains("\"action\"")) return content.Trim();
                return string.Empty;
            }
            catch { return string.Empty; }
        }

        private object BuildRequestBody(string fullQuery, string screenshotBase64, string systemPrompt)
        {
            switch (_settings.LLMProvider.ToLower())
            {
                case "google gemini":
                case "gemini":
                    return BuildGeminiRequest(fullQuery, screenshotBase64, systemPrompt);
                
                case "openai":
                case "chatgpt":
                    return BuildOpenAIRequest(fullQuery, screenshotBase64, systemPrompt);
                
                case "anthropic":
                case "claude":
                    return BuildAnthropicRequest(fullQuery, screenshotBase64, systemPrompt);
                
                case "ollama":
                    return BuildOllamaRequest(fullQuery, screenshotBase64, systemPrompt);
                
                default:
                    // Generic format that works with most APIs
                    return BuildGenericRequest(fullQuery, screenshotBase64, systemPrompt);
            }
        }

        private object BuildGeminiRequest(string fullQuery, string screenshotBase64, string systemPrompt)
        {
            // Build parts array - include text and optionally image
            var parts = new List<object> { new { text = fullQuery } };
            
            if (!string.IsNullOrEmpty(screenshotBase64))
            {
                parts.Add(new { 
                    inline_data = new {
                        mime_type = "image/jpeg",
                        data = screenshotBase64
                    }
                });
            }

            return new
            {
                contents = new[]
                {
                    new
                    {
                        parts = parts.ToArray()
                    }
                },
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = systemPrompt }
                    }
                }
            };
        }

        private object BuildOpenAIRequest(string fullQuery, string screenshotBase64, string systemPrompt)
        {
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = fullQuery }
            };

            // Add image for vision models if screenshot provided
            if (!string.IsNullOrEmpty(screenshotBase64))
            {
                messages[messages.Count - 1] = new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = fullQuery },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{screenshotBase64}" } }
                    }
                };
            }

            return new
            {
                model = _settings.LLMModel,
                messages = messages
            };
        }

        private object BuildAnthropicRequest(string fullQuery, string screenshotBase64, string systemPrompt)
        {
            var content = new List<object> { new { type = "text", text = fullQuery } };
            
            if (!string.IsNullOrEmpty(screenshotBase64))
            {
                content.Add(new
                {
                    type = "image",
                    source = new
                    {
                        type = "base64",
                        media_type = "image/jpeg",
                        data = screenshotBase64
                    }
                });
            }

            return new
            {
                model = _settings.LLMModel,
                max_tokens = 1000,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = content }
                }
            };
        }

        private object BuildOllamaRequest(string fullQuery, string screenshotBase64, string systemPrompt)
        {
            var requestBody = new
            {
                model = _settings.LLMModel,
                prompt = $"{systemPrompt}\n\nUser: {fullQuery}",
                stream = false
            };

            // Note: Ollama vision support varies by model
            if (!string.IsNullOrEmpty(screenshotBase64))
            {
                return new
                {
                    model = _settings.LLMModel,
                    prompt = requestBody.prompt,
                    images = new[] { screenshotBase64 },
                    stream = false
                };
            }

            return requestBody;
        }

        private object BuildGenericRequest(string fullQuery, string screenshotBase64, string systemPrompt)
        {
            // Generic format that works with most OpenAI-compatible APIs
            return BuildOpenAIRequest(fullQuery, screenshotBase64, systemPrompt);
        }

        private string BuildApiUrl()
        {
            switch (_settings.LLMProvider.ToLower())
            {
                case "google gemini":
                case "gemini":
                    return $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={_settings.LLMApiKey}";
                
                case "openai":
                case "chatgpt":
                    return "https://api.openai.com/v1/chat/completions";
                
                case "anthropic":
                case "claude":
                    return "https://api.anthropic.com/v1/messages";
                
                case "ollama":
                    return $"{_settings.LLMBaseUrl}/api/generate";
                
                default:
                    return "https://api.openai.com/v1/chat/completions";
            }
        }

        private void AddAuthorizationHeader()
        {
            // Clear any existing authorization headers
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Remove("x-api-key");

            switch (_settings.LLMProvider.ToLower())
            {
                case "google gemini":
                case "gemini":
                    // Gemini uses API key in URL, no header needed
                    break;
                
                case "openai":
                case "chatgpt":
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.LLMApiKey);
                    break;
                
                case "anthropic":
                case "claude":
                    _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.LLMApiKey);
                    _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                    break;
                
                case "ollama":
                    // Ollama typically doesn't require authentication for local instances
                    if (!string.IsNullOrEmpty(_settings.LLMApiKey))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.LLMApiKey);
                    }
                    break;
                
                default:
                    // Default to Bearer token
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.LLMApiKey);
                    break;
            }
        }

        private string ParseResponse(string responseContent)
        {
            try
            {
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                switch (_settings.LLMProvider.ToLower())
                {
                    case "google gemini":
                    case "gemini":
                        return jsonResponse?.candidates?[0]?.content?.parts?[0]?.text ?? "No response from Gemini";
                    
                    case "openai":
                    case "chatgpt":
                        return jsonResponse?.choices?[0]?.message?.content ?? "No response from OpenAI";
                    
                    case "anthropic":
                    case "claude":
                        return jsonResponse?.content?[0]?.text ?? "No response from Anthropic";
                    
                    case "ollama":
                        return jsonResponse?.response ?? "No response from Ollama";
                    
                    default:
                        // Try common response formats
                        return jsonResponse?.choices?[0]?.message?.content ?? 
                               jsonResponse?.response ?? 
                               jsonResponse?.content ?? 
                               "Unable to parse response";
                }
            }
            catch (Exception ex)
            {
                return $"Failed to parse response: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    internal static class PersonalityProvider
    {
        private static string _cachedPrompt;

        public static string GetSystemPrompt()
        {
            if (!string.IsNullOrEmpty(_cachedPrompt)) return _cachedPrompt;
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(baseDir, "personality.json");
                if (File.Exists(path))
                {
                    dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(path));
                    string prompt = json?.system_prompt;
                    if (!string.IsNullOrWhiteSpace(prompt))
                    {
                        _cachedPrompt = prompt.ToString();
                        return _cachedPrompt;
                    }
                }
            }
            catch { }
            // Fallback
            _cachedPrompt = "You are Chitti. Respond in one single line (no line breaks), max ~30 words, no markdown. Be helpful, concrete, friendly, slightly playful, and professional.";
            return _cachedPrompt;
        }
    }
}