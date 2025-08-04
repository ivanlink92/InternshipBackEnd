using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeerMarking.Migrations
{
    /// <inheritdoc />
    public partial class AddMarkerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Markers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PresentationSlotId = table.Column<int>(type: "int", nullable: false),
                    MarkerStudentId = table.Column<int>(type: "int", nullable: true),
                    TemporaryPassword = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLecturer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Markers_PresentationSlots_PresentationSlotId",
                        column: x => x.PresentationSlotId,
                        principalTable: "PresentationSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Markers_Students_MarkerStudentId",
                        column: x => x.MarkerStudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Markers_MarkerStudentId",
                table: "Markers",
                column: "MarkerStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Markers_PresentationSlotId",
                table: "Markers",
                column: "PresentationSlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Markers");
        }
    }
}
