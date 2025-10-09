using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LineBuddy.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private const string API_KEY = "AIzaSyCbyfJUzzljNXxMlb4uc0X63HXBKehaS2A"; // Replace with actual API key
        private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent";

        public GeminiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> QueryAsync(string query, string conversationContext = "", string screenshotBase64 = "")
        {
            try
            {
                // Combine context with current query if context is provided
                var fullQuery = string.IsNullOrEmpty(conversationContext) 
                    ? query 
                    : $"{conversationContext}{query}";

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

                var requestBody = new
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
                            new { text = "You are Line Buddy, a highly concise, instant AI assistant. Your response MUST be a single sentence with a maximum of 30 words. Do not use markdown, formatting, or line breaks. Answer the user's query directly and briefly." }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.1,
                        maxOutputTokens = 160,
                        topP = 0.8,
                        topK = 10
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{API_URL}?key={API_KEY}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return "API Error - Check your API key";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

                var result = responseObject?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "No response";
                
                // Ensure response doesn't exceed 400 characters (roughly 30 words)
                if (result.Length > 400)
                {
                    result = result.Substring(0, 397) + "...";
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class GeminiResponse
    {
        public Candidate[] Candidates { get; set; }
    }

    public class Candidate
    {
        public Content Content { get; set; }
    }

    public class Content
    {
        public Part[] Parts { get; set; }
    }

    public class Part
    {
        public string Text { get; set; }
    }
}