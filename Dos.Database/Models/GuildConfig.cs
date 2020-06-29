using System.ComponentModel.DataAnnotations.Schema;

namespace Dos.Database.Models
{
    public class GuildConfig
    {
        public int GuildConfigId { get; set; }
        public ulong GuildId { get; set; }
        [Column(TypeName = "jsonb")]
        public BotGameConfig Config { get; set; }
    }
}
