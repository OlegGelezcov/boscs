using System.Collections.Generic;

namespace LeaderboardService.Models
{
    public class Leaderboard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<LeaderboardEntry> Entries { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }
    }
}
