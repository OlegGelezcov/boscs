using LeaderboardService.Models;
using LeaderboardService.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderboardService.Controllers
{
    [Route("api/[controller]")]
    public class LeaderboardController : Controller
    {
        private readonly LeaderboardContext _ctx;

        public LeaderboardController(LeaderboardContext context)
        {
            _ctx = context;
        }

        [HttpGet]
        public List<Leaderboard> Get()
        {
            return _ctx.Leaderboards.ToList();
        }

        [HttpGet("seed")]
        public void Seed()
        {
            _ctx.Database.ExecuteSqlCommand("DELETE FROM LeaderboardEntries");
            _ctx.Database.ExecuteSqlCommand("DELETE FROM Leaderboards");
            _ctx.Database.ExecuteSqlCommand("DELETE FROM Games");

            var tc = new Game() { Name = "Transport Capitalist", Leaderboards = new List<Leaderboard>() };
            var score = new Leaderboard() { Name = "tc_score", Game = tc, GameId = 0, Description = "log(LifetimeSavings)*10000", Entries = new List<LeaderboardEntry>() };

            Random r = new Random();
            for (int i = 0; i < 500; i++)
            {
                var s = r.Next(0, 255) * 10000;

                LeaderboardEntry e = new LeaderboardEntry()
                {
                    DeviceID = $"deviceId_{i}",
                    PlayerName = $"player_{i}",
                    ScoreValue = s
                };

                score.Entries.Add(e);
            }

            tc.Leaderboards.Add(score);

            _ctx.Games.Add(tc);
            _ctx.SaveChanges();
        }

        //GET api/leaderboard/0/score
        [HttpGet("{gameId}/{leaderboardMoniker}")]
        public async Task<ObjectResult> GetLeaderboard(int gameId, string leaderboardMoniker)
        {
            var game = await _ctx.Games.FirstOrDefaultAsync(x => x.Id == gameId);
            if (game == null)
                return BadRequest($"Game with id {gameId} doesn't exist");

            var leaderboard = await _ctx.Leaderboards.Include(nameof(Leaderboard.Entries)).FirstOrDefaultAsync(x => x.Name == leaderboardMoniker);
            if (leaderboard == null)
                return BadRequest($"Leaderboard with moniker {leaderboardMoniker} doesn't exist");

            if (leaderboard.Entries == null)
                return NotFound("null_entries");

            return Ok(leaderboard);
        }

        //GET api/leaderboard/0/score/deviceId_0
        [HttpGet("{gameId}/{leaderboardMoniker}/{id}")]
        public async Task<ObjectResult> Get(int gameId, string leaderboardMoniker, string id)
        {
            var game = await _ctx.Games.FirstOrDefaultAsync(x => x.Id == gameId);
            if (game == null)
                return BadRequest($"Game with id {gameId} doesn't exist");

            var leaderboard = await _ctx.Leaderboards.Include(nameof(Leaderboard.Entries)).FirstOrDefaultAsync(x => x.Name == leaderboardMoniker);
            if (leaderboard == null)
                return BadRequest($"Leaderboard with moniker {leaderboardMoniker} doesn't exist");

            if (leaderboard.Entries == null)
                return NotFound("null_entries");

            var entry = leaderboard.Entries.FirstOrDefault(x => x.DeviceID == id);
            if (entry == null)
                return NotFound("no_entry");

            return Ok(entry);
        }

        //POST api/leaaderboard/0/score
        [HttpPost("{gameId}/{leaderboardMoniker}")]
        public async Task<IActionResult> AddOrReplaceScore(int gameId, string leaderboardMoniker, [FromBody] AddOrUpdateScoreRequest req)
        {
            if (req == null)
                return BadRequest("Empty body");

            var game = await _ctx.Games.FirstOrDefaultAsync(x => x.Id == gameId);
            if (game == null)
                return BadRequest($"Game with id {gameId} doesn't exist");

            var leaderboard = await _ctx.Leaderboards.Include(nameof(Leaderboard.Entries)).FirstOrDefaultAsync(x => x.Name == leaderboardMoniker);
            if (leaderboard == null)
                return BadRequest($"Leaderboard with moniker {leaderboardMoniker} doesn't exist");

            if (leaderboard.Entries == null)
                return NotFound("null entries");

            var existing = leaderboard.Entries.FirstOrDefault(x => x.DeviceID == req.DeviceId);
            if (existing != null)
            {
                if (existing.PlayerName != req.PlayerName)
                    existing.PlayerName = req.PlayerName;

                existing.ScoreValue += req.NewScore;
            }
            else
            {
                var entry = new LeaderboardEntry()
                {
                    DeviceID = req.DeviceId,
                    ScoreValue = req.NewScore,
                    PlayerName = req.PlayerName,
                };

                leaderboard.Entries.Add(entry);
            }

            await _ctx.SaveChangesAsync();

            return Ok();
        }
    }
}
