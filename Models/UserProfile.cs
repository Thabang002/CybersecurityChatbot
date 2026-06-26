namespace CybersecurityChatbot.Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string FavoriteTopic { get; set; }
        public string LastSentiment { get; set; }
        public int QuizScore { get; set; }
        public int QuizAttempts { get; set; }
    }
}