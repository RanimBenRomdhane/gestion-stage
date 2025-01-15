using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_Stagiaire.Data.Migrations
{
    /// <inheritdoc />
    public partial class _008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Titre_Projet",
                table: "DemandesStage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Titre_Projet",
                table: "DemandesStage");
        }
    }
}
