
namespace Assets._Pinball.Scripts.Models
{
    public class PlayerStats
    {
        public PlayerStats()
        {
            Rank = 1;
        }
        public PlayerStats(int highScore, int rank)
        {
            HighScore = highScore;
            Rank = rank;
        }

        public int HighScore { get; set; }
        public int Rank { get; set; }
    }
}
