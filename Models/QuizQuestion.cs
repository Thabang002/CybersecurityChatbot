using System.Collections.Generic;

namespace CybersecurityChatbot.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
        public string Category { get; set; }
    }
}