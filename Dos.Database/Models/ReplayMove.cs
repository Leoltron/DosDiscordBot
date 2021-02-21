using System;
using System.ComponentModel.DataAnnotations.Schema;
using Dos.Game.Logging;
using Dos.Game.Model;

namespace Dos.Database.Models
{
    public class ReplayMove
    {
        public int ReplayMoveId { get; set; }
        public Guid ReplayId { get; set; }
        public GameLogEventType EventType { get; set; }
        public int? SourcePlayer { get; set; }
        public int? TargetPlayer { get; set; }
        [Column(TypeName = "jsonb")]
        public Card[] Cards{ get; set; }

        public virtual Replay Replay { get; set; }
    }
}
