using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Dos.Game.Model;

namespace Dos.Database.Models
{
    public class ReplaySnapshot
    {
        public int ReplaySnapshotId { get; set; }
        public Guid ReplayId { get; set; }
        public int? CurrentPlayerId { get; set; }
        
        [Column(TypeName = "jsonb")]
        public Dictionary<int, Card[]> PlayerHands{ get; set; }

        [Column(TypeName = "jsonb")]
        public Card[][] CenterRow { get; set; }
        
        public virtual Replay Replay { get; set; }
    }
}
