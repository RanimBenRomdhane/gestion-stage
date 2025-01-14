using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_Stagiaire.Data.Migrations
{
    /// <inheritdoc />
    public partial class _007 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AffectationDepartement");

            migrationBuilder.DropTable(
                name: "Affectations");

            migrationBuilder.RenameColumn(
                name: "AffectationId",
                table: "DemandesStage",
                newName: "DepartementId");

            migrationBuilder.AddColumn<string>(
                name: "Encadrant",
                table: "DemandesStage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesStage_DepartementId",
                table: "DemandesStage",
                column: "DepartementId");

            migrationBuilder.AddForeignKey(
                name: "FK_DemandesStage_Departements_DepartementId",
                table: "DemandesStage",
                column: "DepartementId",
                principalTable: "Departements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandesStage_Departements_DepartementId",
                table: "DemandesStage");

            migrationBuilder.DropIndex(
                name: "IX_DemandesStage_DepartementId",
                table: "DemandesStage");

            migrationBuilder.DropColumn(
                name: "Encadrant",
                table: "DemandesStage");

            migrationBuilder.RenameColumn(
                name: "DepartementId",
                table: "DemandesStage",
                newName: "AffectationId");

            migrationBuilder.CreateTable(
                name: "Affectations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DemandeStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date_Affectation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Encadrant = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Affectations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Affectations_DemandesStage_DemandeStageId",
                        column: x => x.DemandeStageId,
                        principalTable: "DemandesStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AffectationDepartement",
                columns: table => new
                {
                    AffectationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffectationDepartement", x => new { x.AffectationsId, x.DepartementId });
                    table.ForeignKey(
                        name: "FK_AffectationDepartement_Affectations_AffectationsId",
                        column: x => x.AffectationsId,
                        principalTable: "Affectations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AffectationDepartement_Departements_DepartementId",
                        column: x => x.DepartementId,
                        principalTable: "Departements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AffectationDepartement_DepartementId",
                table: "AffectationDepartement",
                column: "DepartementId");

            migrationBuilder.CreateIndex(
                name: "IX_Affectations_DemandeStageId",
                table: "Affectations",
                column: "DemandeStageId",
                unique: true);
        }
    }
}
