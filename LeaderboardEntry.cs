namespace LeaderboardService.Models
{
    public class LeaderboardEntry
    {
        public int Id { get; set; }
        public string DeviceID { get; set; }
        public string PlayerName { get; set; }
        public double ScoreValue { get; set; }
    }
}
