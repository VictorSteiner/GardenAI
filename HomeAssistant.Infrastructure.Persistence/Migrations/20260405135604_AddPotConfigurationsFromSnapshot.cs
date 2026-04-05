using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPotConfigurationsFromSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PotConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PotId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomAreaId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RoomName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CurrentSeeds = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PotConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PotConfigurations_PotId",
                table: "PotConfigurations",
                column: "PotId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PotConfigurations");
        }
    }
}
