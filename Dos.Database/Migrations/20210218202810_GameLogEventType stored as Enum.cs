using Microsoft.EntityFrameworkCore.Migrations;

namespace Dos.Database.Migrations
{
    public partial class GameLogEventTypestoredasEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:game_log_event_type", "player_received_card,center_row_match,center_row_add,center_row_player_add,clear_center_row,player_turn_start,player_go_out,players_swapped_hands");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:game_log_event_type", "player_received_card,center_row_match,center_row_add,center_row_player_add,clear_center_row,player_turn_start,player_go_out,players_swapped_hands");
        }
    }
}
