using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClearPersistedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear all persisted data while preserving schema and migrations history
            // Order matters: delete dependent tables first due to foreign key constraints

            // Delete chat messages first (dependent on chat sessions)
            migrationBuilder.Sql("DELETE FROM chat_messages;");

            // Delete chat sessions
            migrationBuilder.Sql("DELETE FROM chat_sessions;");

            // Delete sensor readings (dependent on plant pots)
            migrationBuilder.Sql("DELETE FROM sensor_readings;");

            // Delete pot configurations (dependent on plant pots)
            migrationBuilder.Sql("DELETE FROM pot_configurations;");

            // Delete plant pots
            migrationBuilder.Sql("DELETE FROM plant_pots;");

            // Delete plant species (if not referenced by anything else)
            migrationBuilder.Sql("DELETE FROM plant_species;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No automatic rollback for data deletion. This is intentional.
            // If you need to restore data, use a backup from before this migration.
        }
    }
}

