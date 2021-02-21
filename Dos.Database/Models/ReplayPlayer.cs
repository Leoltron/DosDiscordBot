using System;

namespace Dos.Database.Models
{
    public class ReplayPlayer
    {
        public int ReplayPlayerId { get; set; }
        public Guid ReplayId { get; set; }
        public int OrderId { get; set; }
        public string PlayerName { get; set; }
        
        public virtual Replay Replay { get; set; }
    }
}
