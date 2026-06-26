using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    public class ChatbotService
    {
        private Dictionary<string, List<string>> keywordResponses;
        private Dictionary<string, string> topicFollowUp;
        private UserProfile userProfile;
        private string currentTopic;
        private ActivityLogService logService;

        private string[] greetings = {
            "Hello! I'm your Cybersecurity Awareness Assistant. 🇿🇦",
            "Welcome! I'm here to help you stay safe online.",
            "Hi there! Ready to learn about cybersecurity?"
        };

        private string[] farewells = {
            "Stay safe online! Remember, cybersecurity is everyone's responsibility.",
            "Goodbye! Keep your digital life secure.",
            "Take care! Feel free to come back if you have more questions."
        };

        public ChatbotService(ActivityLogService logService)
        {
            this.logService = logService;
            userProfile = new UserProfile();
            InitializeKeywordResponses();
            InitializeTopicFollowUp();
        }

        private void InitializeKeywordResponses()
        {
            keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new List<string>
                {
                    "🔐 Use strong, unique passwords for each account. Consider using a password manager!",
                    "💡 Avoid using personal information like birthdays in your passwords.",
                    "🛡️ Enable two-factor authentication whenever possible for extra security.",
                    "📝 Change your passwords regularly, especially for sensitive accounts."
                },
                ["scam"] = new List<string>
                {
                    "⚠️ Be wary of unsolicited emails or calls asking for personal information.",
                    "🔍 Always verify the source before sharing any sensitive information.",
                    "📧 Never click on links in suspicious emails. Check the sender's address carefully.",
                    "🛑 If something seems too good to be true, it probably is a scam."
                },
                ["privacy"] = new List<string>
                {
                    "🔒 Review your privacy settings on social media regularly.",
                    "📱 Be cautious about what personal information you share online.",
                    "🌐 Use privacy-focused browsers and search engines when possible.",
                    "🔐 Enable encryption on your devices and communications."
                },
                ["phishing"] = new List<string>
                {
                    "🎣 Phishing emails often create a sense of urgency. Always verify before acting!",
                    "📧 Look for spelling mistakes and suspicious email addresses in phishing attempts.",
                    "🔗 Hover over links to see the actual URL before clicking.",
                    "🚨 Report phishing emails to your IT department or security team."
                },
                ["2fa"] = new List<string>
                {
                    "🔑 Two-factor authentication adds an extra layer of security to your accounts.",
                    "📱 Use authenticator apps instead of SMS for more secure 2FA.",
                    "🛡️ Enable 2FA on all accounts that support it, especially email and banking.",
                    "📋 Keep backup codes in a safe place when setting up 2FA."
                },
                ["update"] = new List<string>
                {
                    "🔄 Keep all your software and apps updated to patch security vulnerabilities.",
                    "📱 Enable automatic updates whenever possible.",
                    "🔐 Security updates are crucial for protecting against new threats.",
                    "💻 Update your operating system regularly for the best protection."
                },
                ["social engineering"] = new List<string>
                {
                    "🧠 Social engineering exploits human psychology. Always think before sharing info!",
                    "📞 Verify caller identity before giving out information over the phone.",
                    "👥 Be cautious about what you share on social media that could be used against you.",
                    "🔒 Use a code word with family/friends for emergency verification."
                },
                ["help"] = new List<string>
                {
                    "I can help you with cybersecurity topics! Try asking about: passwords, scams, privacy, phishing, 2FA, updates, or social engineering.",
                    "💡 Need assistance? Ask me about any cybersecurity topic you're interested in.",
                    "🛡️ I'm here to help! What would you like to know about online safety?"
                }
            };
        }

        private void InitializeTopicFollowUp()
        {
            topicFollowUp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = "Would you like to know more about creating strong passwords or using a password manager?",
                ["scam"] = "Would you like more tips on identifying and avoiding common scams?",
                ["privacy"] = "Would you like to learn more about protecting your privacy online?",
                ["phishing"] = "Would you like to know how to identify phishing attempts better?",
                ["2fa"] = "Would you like to learn how to set up 2FA on your accounts?",
                ["update"] = "Would you like to know how to keep your devices updated automatically?",
                ["social engineering"] = "Would you like to learn more about protecting yourself from social engineering attacks?"
            };
        }

        public string ProcessMessage(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "Please enter a message. I'm here to help you learn about cybersecurity!";

            var input = userInput.Trim();

            // Check for activity log command
            if (input.Equals("show activity log", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("what have you done for me", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("show log", StringComparison.OrdinalIgnoreCase))
            {
                return GetActivityLogResponse();
            }

            // Check for greetings
            if (IsGreeting(input))
                return GetGreetingResponse();

            // Check for farewells
            if (IsFarewell(input))
                return GetFarewellResponse();

            // Check for follow-up requests
            if (IsFollowUpRequest(input))
                return GetFollowUpResponse();

            // Check for sentiment
            var sentiment = DetectSentiment(input);
            if (sentiment != "neutral")
                userProfile.LastSentiment = sentiment;

            // Check for user details
            if (input.ToLower().Contains("my name is") || input.ToLower().Contains("i am "))
            {
                var name = ExtractName(input);
                if (!string.IsNullOrEmpty(name))
                {
                    userProfile.Name = name;
                    logService.LogActivity("User Info", $"User provided name: {name}");
                    return $"Nice to meet you, {name}! I'll remember that. How can I help you with cybersecurity today?";
                }
            }

            // Check for favorite topic
            if (input.ToLower().Contains("i'm interested in") || input.ToLower().Contains("i am interested in"))
            {
                var topic = ExtractTopic(input);
                if (!string.IsNullOrEmpty(topic))
                {
                    userProfile.FavoriteTopic = topic;
                    logService.LogActivity("User Info", $"User interested in: {topic}");
                    return $"Great! I'll remember that you're interested in {topic}. It's a crucial part of staying safe online.";
                }
            }

            // Check for current topic follow-up
            if (!string.IsNullOrEmpty(currentTopic))
            {
                var response = GetResponseForTopic(input);
                if (!string.IsNullOrEmpty(response))
                    return response;
            }

            // General keyword matching
            string detectedTopic = DetectTopic(input);
            if (!string.IsNullOrEmpty(detectedTopic))
            {
                currentTopic = detectedTopic;
                var response = GetRandomResponseForTopic(detectedTopic);
                logService.LogActivity("Chat", $"User asked about: {detectedTopic}");
                return response;
            }

            // Unknown input
            logService.LogActivity("Chat", "User asked an unrecognized question");
            return GetDefaultResponse();
        }

        private string GetActivityLogResponse()
        {
            var logs = DatabaseService.GetRecentActivityLogs(10);
            if (logs.Count == 0)
                return "I haven't logged any activities yet. Start using my features to see them here!";

            var response = "📋 Here's a summary of recent actions:\n\n";
            for (int i = 0; i < logs.Count; i++)
            {
                response += $"{i + 1}. {logs[i].Action}: {logs[i].Description}\n";
                response += $"   {logs[i].Timestamp.ToString("MMM dd, HH:mm")}\n";
            }
            return response;
        }

        private bool IsGreeting(string input)
        {
            string[] greetings = { "hello", "hi", "hey", "good morning", "good afternoon", "good evening", "howdy" };
            return greetings.Any(g => input.ToLower().StartsWith(g));
        }

        private bool IsFarewell(string input)
        {
            string[] farewells = { "bye", "goodbye", "see you", "farewell", "take care", "exit", "quit" };
            return farewells.Any(f => input.ToLower().StartsWith(f) || input.ToLower().Contains(f));
        }

        private bool IsFollowUpRequest(string input)
        {
            string[] followUp = { "tell me more", "explain more", "another tip", "give me another", "more", "continue", "next" };
            return followUp.Any(f => input.ToLower().Contains(f));
        }

        private string GetGreetingResponse()
        {
            var random = new Random();
            return greetings[random.Next(greetings.Length)] + " 🇿🇦\n" +
                   "I can help you with: passwords, scams, privacy, phishing, 2FA, updates, and more!";
        }

        private string GetFarewellResponse()
        {
            var random = new Random();
            return farewells[random.Next(farewells.Length)];
        }

        private string GetFollowUpResponse()
        {
            if (string.IsNullOrEmpty(currentTopic))
            {
                string[] followUps = {
                    "I'd be happy to help! What cybersecurity topic would you like to learn about?",
                    "Sure! What specific area of cybersecurity interests you?",
                    "Of course! What would you like to know more about?"
                };
                return followUps[new Random().Next(followUps.Length)];
            }

            var responses = keywordResponses.FirstOrDefault(k =>
                k.Key.Equals(currentTopic, StringComparison.OrdinalIgnoreCase));

            if (responses.Key != null && responses.Value.Count > 0)
            {
                var random = new Random();
                var response = responses.Value[random.Next(responses.Value.Count)];
                return $"{response}\n\n{GetFollowUpQuestion(currentTopic)}";
            }

            return "I'd be happy to share more! What specific topic would you like me to elaborate on?";
        }

        private string GetFollowUpQuestion(string topic)
        {
            return topicFollowUp.TryGetValue(topic, out var question) ? question :
                   $"Would you like to know more about {topic}?";
        }

        private string DetectSentiment(string input)
        {
            if (input.ToLower().Contains("worried") || input.ToLower().Contains("scared") ||
                input.ToLower().Contains("concerned") || input.ToLower().Contains("anxious") ||
                input.ToLower().Contains("afraid"))
                return "worried";

            if (input.ToLower().Contains("frustrated") || input.ToLower().Contains("annoyed") ||
                input.ToLower().Contains("tired"))
                return "frustrated";

            if (input.ToLower().Contains("curious") || input.ToLower().Contains("interest") ||
                input.ToLower().Contains("want to know"))
                return "curious";

            return "neutral";
        }

        private string ExtractName(string input)
        {
            var match = Regex.Match(input, @"(?i)(?:my name is|i am|i'm)\s+(\w+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private string ExtractTopic(string input)
        {
            var match = Regex.Match(input, @"(?i)(?:interested in|learn about|know about)\s+(\w+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private string DetectTopic(string input)
        {
            foreach (var keyword in keywordResponses.Keys)
            {
                if (input.ToLower().Contains(keyword.ToLower()))
                    return keyword;
            }
            return null;
        }

        private string GetResponseForTopic(string input)
        {
            // If user asks a specific question about current topic
            if (input.ToLower().Contains("how") || input.ToLower().Contains("what") ||
                input.ToLower().Contains("why") || input.ToLower().Contains("when"))
            {
                var response = GetRandomResponseForTopic(currentTopic);
                if (!string.IsNullOrEmpty(response))
                    return response;
            }
            return null;
        }

        private string GetRandomResponseForTopic(string topic)
        {
            if (keywordResponses.TryGetValue(topic, out var responses) && responses.Count > 0)
            {
                var random = new Random();
                var response = responses[random.Next(responses.Count)];

                // Adjust response based on sentiment
                if (userProfile.LastSentiment == "worried")
                    return $"I understand your concern. {response}\n\n" +
                           "Remember, staying informed is the first step to staying safe online!";
                if (userProfile.LastSentiment == "frustrated")
                    return $"I hear your frustration. Let me help you with that: {response}\n\n" +
                           "Take your time - cybersecurity learning is a journey!";
                if (userProfile.LastSentiment == "curious")
                    return $"It's great that you're curious! {response}\n\n" +
                           "Would you like to dive deeper into this topic?";

                // Personalize based on user name
                if (!string.IsNullOrEmpty(userProfile.Name))
                    return $"{userProfile.Name}, {response}";

                return response;
            }
            return null;
        }

        private string GetDefaultResponse()
        {
            string[] defaults = {
                "I'm not sure I understand. Can you try rephrasing?",
                "I didn't catch that. Could you ask about a cybersecurity topic like passwords, scams, or privacy?",
                "I'm not sure about that. Try asking me about password safety, phishing, or online privacy!",
                "I'm still learning! Could you ask about a specific cybersecurity topic?",
                "Let me help you with that. What specific cybersecurity topic are you interested in?"
            };
            return defaults[new Random().Next(defaults.Length)];
        }

        public string GetCurrentTopic() => currentTopic;
        public UserProfile GetUserProfile() => userProfile;
    }
}