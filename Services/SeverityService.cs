using System.Text.RegularExpressions;

namespace ThikaResQNet.Services
{
    public class SeverityService : ISeverityService
    {
        private static readonly string[] HighKeywords = new[] { "bleeding", "unconscious", "not breathing", "no pulse", "severe", "heart attack", "stroke", "cardiac arrest", "amputation", "major trauma" };
        private static readonly string[] MediumKeywords = new[] { "injury", "fracture", "burn", "broken bone", "dizziness", "concussion", "moderate", "breathing difficulty" };
        private static readonly string[] LowKeywords = new[] { "minor", "sprain", "scratch", "small cut", "nausea", "headache", "pain" };

        public string CalculateSeverity(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Low";

            var text = description.ToLowerInvariant();

            // High-priority keyword match
            foreach (var phrase in HighKeywords)
            {
                if (text.Contains(phrase)) return "High";
            }

            // Heuristic checks for heavy bleeding or blood volume indicators
            if (Regex.IsMatch(text, @"\b(\d+\s?(ml|l|litres?|liters?)|heavy(ly)? bleeding|profuse bleeding|blood everywhere)\b"))
            {
                return "High";
            }

            // Medium-priority keyword match
            foreach (var phrase in MediumKeywords)
            {
                if (text.Contains(phrase)) return "Medium";
            }

            // Low-priority keyword match
            foreach (var phrase in LowKeywords)
            {
                if (text.Contains(phrase)) return "Low";
            }

            // Fallback heuristic: presence of exclamation or urgent words
            if (text.Contains("help") || text.Contains("urgent") || text.Contains("please help") || text.Contains("!"))
            {
                return "High";
            }

            return "Low";
        }
    }
}