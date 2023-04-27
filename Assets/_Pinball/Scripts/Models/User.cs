
namespace Assets._Pinball.Scripts.Models
{
    public class User
    {
        public User(string playerId, string username, string imagePath, int highScore, string rank)
        {
            PlayerId = playerId;
            Username = username;
            ImagePath = imagePath;
            HighScore = highScore;
            Rank = rank;
        }

        public string PlayerId { get; set; }
        public string Username { get; set; }
        public string ImagePath { get; set; }
        public int HighScore { get; set; }
        public string Rank { get; set; }
    }
}
