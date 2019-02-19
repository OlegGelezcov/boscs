using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderboardService.Models.Requests
{
    public class AddOrUpdateScoreRequest
    {
        public string DeviceId { get; set; }
        public string PlayerName { get; set; }
        public double NewScore { get; set; }
    }
}
