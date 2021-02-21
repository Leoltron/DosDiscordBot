using System;
using System.Collections.Generic;

namespace Dos.Database.Models
{
    public class Replay
    {
        public Guid ReplayId { get; set; }
        public DateTime GameStartDate { get; set; }
        public string GuildTitle { get; set; }
        public string ChannelTitle { get; set; }
        public bool IsOngoing { get; set; }
        public bool IsPublic { get; set; }
        
        public virtual ICollection<ReplayPlayer> Players { get; set; }
        public virtual ICollection<ReplayMove> Moves { get; set; }
        public virtual ICollection<ReplaySnapshot> Snapshots { get; set; }
    }
}
