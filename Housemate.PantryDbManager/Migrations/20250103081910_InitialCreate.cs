using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Housemate.PantryDbManager.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence(
            name: "pantry_category_hilo",
            incrementBy: 10);

        migrationBuilder.CreateSequence(
            name: "pantry_hilo",
            incrementBy: 10);

        migrationBuilder.CreateSequence(
            name: "pantry_location_hilo",
            incrementBy: 10);

        migrationBuilder.CreateTable(
            name: "PantryCategory",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false),
                Category = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PantryCategory", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PantryLocation",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false),
                Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                IsFridge = table.Column<bool>(type: "boolean", nullable: false),
                IsFreezer = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PantryLocation", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Pantry",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Quantity = table.Column<float>(type: "real", precision: 5, scale: 1, nullable: false),
                Size = table.Column<float>(type: "real", precision: 5, scale: 1, nullable: true),
                Units = table.Column<string>(type: "text", nullable: true),
                PantryLocationId = table.Column<int>(type: "integer", nullable: false),
                PantryCategoryId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pantry", x => x.Id);
                table.ForeignKey(
                    name: "FK_Pantry_PantryCategory_PantryCategoryId",
                    column: x => x.PantryCategoryId,
                    principalTable: "PantryCategory",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Pantry_PantryLocation_PantryLocationId",
                    column: x => x.PantryLocationId,
                    principalTable: "PantryLocation",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Pantry_PantryCategoryId",
            table: "Pantry",
            column: "PantryCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Pantry_PantryLocationId",
            table: "Pantry",
            column: "PantryLocationId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Pantry");

        migrationBuilder.DropTable(
            name: "PantryCategory");

        migrationBuilder.DropTable(
            name: "PantryLocation");

        migrationBuilder.DropSequence(
            name: "pantry_category_hilo");

        migrationBuilder.DropSequence(
            name: "pantry_hilo");

        migrationBuilder.DropSequence(
            name: "pantry_location_hilo");
    }
}
