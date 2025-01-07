using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_Stagiaire.Data.Migrations
{
    /// <inheritdoc />
    public partial class _001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stagiaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telephone = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ecole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path_Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path_CV = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stagiaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DemandesStage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StagiaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type_Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date_Debut = table.Column<DateOnly>(type: "date", nullable: false),
                    Date_Fin = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path_Demande_Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date_Demande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Affectation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandesStage_Stagiaires_StagiaireId",
                        column: x => x.StagiaireId,
                        principalTable: "Stagiaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemandesStage_StagiaireId",
                table: "DemandesStage",
                column: "StagiaireId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemandesStage");

            migrationBuilder.DropTable(
                name: "Stagiaires");
        }
    }
}
