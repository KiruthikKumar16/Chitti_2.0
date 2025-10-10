using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LineBuddy.Models
{
    public class Personality
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        [JsonProperty("display_name")]
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Voice { get; set; } = "";
        public string Tone { get; set; } = "";
        public List<string> Catchphrases { get; set; } = new List<string>();
        [JsonProperty("system_prompt")]
        public string SystemPrompt { get; set; } = "";
        [JsonProperty("message_styles")]
        public Dictionary<string, string> MessageStyles { get; set; } = new Dictionary<string, string>();
        public PersonalityConstraints Constraints { get; set; } = new PersonalityConstraints();
    }

    public class PersonalityConstraints
    {
        [JsonProperty("max_words")]
        public int MaxWords { get; set; } = 30;
        [JsonProperty("single_line")]
        public bool SingleLine { get; set; } = true;
        public string Formality { get; set; } = "casual"; // casual, medium, high
    }
}
