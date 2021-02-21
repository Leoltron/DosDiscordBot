using System;
using System.Collections.Generic;
using Dos.Game.Model;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Dos.Database.Migrations
{
    public partial class Replays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Replay",
                columns: table => new
                {
                    ReplayId = table.Column<Guid>(nullable: false),
                    GameStartDate = table.Column<DateTime>(nullable: false),
                    GuildTitle = table.Column<string>(nullable: true),
                    ChannelTitle = table.Column<string>(nullable: true),
                    IsOngoing = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replay", x => x.ReplayId);
                });

            migrationBuilder.CreateTable(
                name: "ReplayMove",
                columns: table => new
                {
                    ReplayMoveId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReplayId = table.Column<Guid>(nullable: false),
                    EventType = table.Column<int>(nullable: false),
                    SourcePlayer = table.Column<int>(nullable: true),
                    TargetPlayer = table.Column<int>(nullable: true),
                    Cards = table.Column<Card[]>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayMove", x => x.ReplayMoveId);
                    table.ForeignKey(
                        name: "FK_ReplayMove_Replay_ReplayId",
                        column: x => x.ReplayId,
                        principalTable: "Replay",
                        principalColumn: "ReplayId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReplayPlayer",
                columns: table => new
                {
                    ReplayPlayerId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReplayId = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<int>(nullable: false),
                    PlayerName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayPlayer", x => x.ReplayPlayerId);
                    table.ForeignKey(
                        name: "FK_ReplayPlayer_Replay_ReplayId",
                        column: x => x.ReplayId,
                        principalTable: "Replay",
                        principalColumn: "ReplayId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReplaySnapshot",
                columns: table => new
                {
                    ReplaySnapshotId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReplayId = table.Column<Guid>(nullable: false),
                    CurrentPlayerId = table.Column<int>(nullable: true),
                    PlayerHands = table.Column<Dictionary<int, Card[]>>(type: "jsonb", nullable: true),
                    CenterRow = table.Column<Card[][]>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplaySnapshot", x => x.ReplaySnapshotId);
                    table.ForeignKey(
                        name: "FK_ReplaySnapshot_Replay_ReplayId",
                        column: x => x.ReplayId,
                        principalTable: "Replay",
                        principalColumn: "ReplayId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Replay_GameStartDate",
                table: "Replay",
                column: "GameStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayMove_ReplayId",
                table: "ReplayMove",
                column: "ReplayId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayer_ReplayId",
                table: "ReplayPlayer",
                column: "ReplayId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplaySnapshot_ReplayId",
                table: "ReplaySnapshot",
                column: "ReplayId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReplayMove");

            migrationBuilder.DropTable(
                name: "ReplayPlayer");

            migrationBuilder.DropTable(
                name: "ReplaySnapshot");

            migrationBuilder.DropTable(
                name: "Replay");
        }
    }
}
