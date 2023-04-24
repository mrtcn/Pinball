
namespace Assets._Pinball.Scripts.Models
{
    public class User
    {
        public User(string playerId, string username)
        {
            PlayerId = playerId;
            Username = username;
        }

        public string PlayerId { get; set; }
        public string Username { get; set; }
    }
}
