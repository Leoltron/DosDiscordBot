using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dos.Database;
using Dos.ReplayService.Models;
using Microsoft.EntityFrameworkCore;

namespace Dos.ReplayService.Pages
{
    public class ReplayListModel : PageModel
    {
        private readonly ILogger<ReplayListModel> _logger;
        private readonly DosDbContext context;

        public ReplayListModel(ILogger<ReplayListModel> logger, DosDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public IList<ReplayListItemViewModel> Replays { get; private set; }

        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> OnGet()
        {
            Replays = await context.Replay
                                   .Where(r => r.IsPublic && !r.IsOngoing)
                                   .OrderByDescending(r => r.GameStartDate)
                                   .Take(20)
                                   .Select(r => new ReplayListItemViewModel(
                                               r.GuildTitle,
                                               r.ChannelTitle,
                                               r.ReplayId,
                                               new DateTime(r.GameStartDate.Ticks, DateTimeKind.Utc),
                                               r.Players.Count))
                                   .ToListAsync();
            
            return Page();
        }
    }
}
