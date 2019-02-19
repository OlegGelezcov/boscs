using System.Collections.Generic;

namespace LeaderboardService.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Leaderboard> Leaderboards { get; set; }
    }
}
