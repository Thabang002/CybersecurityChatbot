using System;
using System.Collections.Generic;
using System.Linq;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    public class QuizService
    {
        private List<QuizQuestion> questions;
        private int currentQuestionIndex;
        private int score;
        private bool isActive;
        private ActivityLogService logService;

        public bool IsActive => isActive;
        public int TotalQuestions => questions.Count;
        public int CurrentQuestionNumber => currentQuestionIndex + 1;
        public QuizQuestion CurrentQuestion => isActive && currentQuestionIndex < questions.Count ? questions[currentQuestionIndex] : null;

        public QuizService(ActivityLogService logService)
        {
            this.logService = logService;
            InitializeQuestions();
            isActive = false;
            currentQuestionIndex = 0;
            score = 0;
        }

        private void InitializeQuestions()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Id = 1,
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Reporting phishing emails helps prevent scams and protects others from falling victim.",
                    Category = "Phishing"
                },
                new QuizQuestion
                {
                    Id = 2,
                    Question = "Which of the following is a strong password?",
                    Options = new List<string> { "123456", "password123", "P@ssw0rd!2024", "qwerty" },
                    CorrectAnswerIndex = 2,
                    Explanation = "A strong password includes a mix of uppercase, lowercase, numbers, and special characters.",
                    Category = "Password Safety"
                },
                new QuizQuestion
                {
                    Id = 3,
                    Question = "True or False: It's safe to use public Wi-Fi for online banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Public Wi-Fi networks are not secure. Always use a VPN or cellular data for sensitive transactions.",
                    Category = "Safe Browsing"
                },
                new QuizQuestion
                {
                    Id = 4,
                    Question = "What is two-factor authentication (2FA)?",
                    Options = new List<string> { "A type of password", "A second layer of security", "A virus", "A type of scam" },
                    CorrectAnswerIndex = 1,
                    Explanation = "2FA adds an extra security layer by requiring a second form of verification beyond your password.",
                    Category = "Authentication"
                },
                new QuizQuestion
                {
                    Id = 5,
                    Question = "What is the most common type of cyber attack?",
                    Options = new List<string> { "Phishing", "Malware", "Ransomware", "DDoS" },
                    CorrectAnswerIndex = 0,
                    Explanation = "Phishing is the most common attack, often delivered through email or text messages.",
                    Category = "Cyber Attacks"
                },
                new QuizQuestion
                {
                    Id = 6,
                    Question = "True or False: Antivirus software is only needed for computers, not phones.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Mobile devices also need protection against malware and other threats.",
                    Category = "Security Software"
                },
                new QuizQuestion
                {
                    Id = 7,
                    Question = "What should you do before clicking a link in an email?",
                    Options = new List<string> { "Click it immediately", "Hover over it to see the URL", "Forward it to friends", "Ignore it completely" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Hovering over a link shows the actual URL, helping you identify potential phishing attempts.",
                    Category = "Safe Browsing"
                },
                new QuizQuestion
                {
                    Id = 8,
                    Question = "Which of these is a sign of a phishing email?",
                    Options = new List<string> { "Spelling mistakes", "Urgent action required", "Suspicious sender address", "All of the above" },
                    CorrectAnswerIndex = 3,
                    Explanation = "Phishing emails often have multiple red flags. Be suspicious of any email that seems off.",
                    Category = "Phishing"
                },
                new QuizQuestion
                {
                    Id = 9,
                    Question = "What is social engineering?",
                    Options = new List<string> { "A type of software", "Manipulating people into giving information", "Building social media", "A security tool" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Social engineering exploits human psychology rather than technical vulnerabilities.",
                    Category = "Social Engineering"
                },
                new QuizQuestion
                {
                    Id = 10,
                    Question = "True or False: You should use the same password for all accounts.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Using unique passwords for each account prevents a single breach from compromising all your accounts.",
                    Category = "Password Safety"
                },
                new QuizQuestion
                {
                    Id = 11,
                    Question = "What is the best way to protect your personal data online?",
                    Options = new List<string> { "Share only with trusted sites", "Use privacy settings", "Regularly review permissions", "All of the above" },
                    CorrectAnswerIndex = 3,
                    Explanation = "A combination of these practices provides the best protection for your personal data.",
                    Category = "Privacy"
                },
                new QuizQuestion
                {
                    Id = 12,
                    Question = "True or False: Ransomware only affects large companies.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Ransomware can affect anyone. Regular backups are essential protection against this threat.",
                    Category = "Ransomware"
                }
            };
        }

        public string StartQuiz()
        {
            isActive = true;
            currentQuestionIndex = 0;
            score = 0;
            logService.LogActivity("Quiz Started", "User started the cybersecurity quiz");
            return GetNextQuestion();
        }

        public string GetNextQuestion()
        {
            if (!isActive || currentQuestionIndex >= questions.Count)
            {
                return EndQuiz();
            }

            var question = questions[currentQuestionIndex];
            var response = $"📝 **Question {currentQuestionIndex + 1} of {questions.Count}**\n";
            response += $"Category: {question.Category}\n\n";
            response += $"{question.Question}\n\n";

            for (int i = 0; i < question.Options.Count; i++)
            {
                response += $"{(char)('A' + i)}) {question.Options[i]}\n";
            }

            response += $"\nType your answer (A, B, C, or D)";
            return response;
        }

        public string ProcessAnswer(string answer)
        {
            if (!isActive)
                return "The quiz is not active. Type 'Start quiz' to begin!";

            if (currentQuestionIndex >= questions.Count)
                return EndQuiz();

            var question = questions[currentQuestionIndex];
            var input = answer.ToUpper().Trim();

            // Convert letter to index
            int selectedIndex = -1;
            if (input.Length == 1 && input[0] >= 'A' && input[0] <= 'D')
            {
                selectedIndex = input[0] - 'A';
            }
            else if (int.TryParse(answer, out int numIndex) && numIndex >= 1 && numIndex <= question.Options.Count)
            {
                selectedIndex = numIndex - 1;
            }
            else
            {
                // Try to match the answer text
                for (int i = 0; i < question.Options.Count; i++)
                {
                    if (question.Options[i].Equals(answer, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            if (selectedIndex == -1 || selectedIndex >= question.Options.Count)
                return "Invalid answer. Please choose A, B, C, or D.";

            bool isCorrect = selectedIndex == question.CorrectAnswerIndex;
            if (isCorrect)
                score++;

            var response = isCorrect ? "✅ **Correct!** " : "❌ **Incorrect.** ";
            response += $"\n{question.Explanation}\n\n";

            // Show the correct answer if wrong
            if (!isCorrect)
            {
                response += $"The correct answer was: {question.Options[question.CorrectAnswerIndex]}\n\n";
            }

            currentQuestionIndex++;
            logService.LogActivity("Quiz Progress", $"Question {currentQuestionIndex}: {(isCorrect ? "Correct" : "Incorrect")}");

            if (currentQuestionIndex >= questions.Count)
            {
                response += EndQuiz();
            }
            else
            {
                response += GetNextQuestion();
            }

            return response;
        }

        public string EndQuiz()
        {
            isActive = false;
            var percentage = (double)score / questions.Count * 100;
            var feedback = percentage >= 80 ? "🌟 Excellent! You're a cybersecurity pro!" :
                          percentage >= 60 ? "💪 Good job! Keep learning to become a pro!" :
                          "📚 Good attempt! Review these topics and try again!";

            var response = "🏁 **Quiz Complete!**\n\n";
            response += $"Your score: {score}/{questions.Count} ({percentage:F1}%)\n\n";
            response += feedback + "\n\n";
            response += "Would you like to try again? Type 'Start quiz' to retake!";

            DatabaseService.SaveQuizResult(score, questions.Count);
            logService.LogActivity("Quiz Completed", $"Score: {score}/{questions.Count}");

            return response;
        }

        public string GetQuizProgress()
        {
            if (!isActive)
                return "No quiz in progress. Type 'Start quiz' to begin!";

            return $"📊 Quiz Progress: {currentQuestionIndex}/{questions.Count} questions answered, Score: {score}";
        }
    }
}