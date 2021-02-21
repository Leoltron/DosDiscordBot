using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dos.Database;
using Dos.Game.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Dos.ReplayService.Pages
{
    public class Replay : PageModel
    {
        private readonly DosDbContext context;

        public Replay(DosDbContext context)
        {
            this.context = context;
        }

        public Guid GameId { get; private set; }
        public int MoveId { get; private set; }
        public GameReplay GameReplay { get; private set; }

        public string Title { get; private set; }

        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> OnGet(Guid gameId, int moveId = 0)
        {
            if (gameId == Guid.Empty)
                return NotFound();

            GameId = gameId;
            MoveId = moveId;

            var replay = await context.Replay.AsQueryable()
                                      .Include(r => r.Moves)
                                      .Include(r => r.Players)
                                      .Include(r => r.Snapshots)
                                      .FirstOrDefaultAsync(r => r.ReplayId == gameId);

            if (replay == null)
                return NotFound();

            GameReplay = new GameReplay(replay.Players.Select(p => p.PlayerName).ToArray(),
                                        replay.Moves
                                              .Select(m => new GameLogEvent(
                                                          m.EventType,
                                                          m.SourcePlayer,
                                                          m.TargetPlayer,
                                                          m.Cards)).ToArray(),
                                        replay.Snapshots
                                              .Select(s => new GameSnapshot(
                                                          s.CurrentPlayerId,
                                                          s.PlayerHands,
                                                          s.CenterRow)).ToArray(),
                                        replay.GameStartDate
            );

            var startDateUtc = new DateTime(replay.GameStartDate.Ticks, DateTimeKind.Utc);

            Title =
                $"{replay.GuildTitle}#{replay.ChannelTitle} - {startDateUtc.ToString("g", CultureInfo.InvariantCulture)}";

            return Page();
        }
    }
}
