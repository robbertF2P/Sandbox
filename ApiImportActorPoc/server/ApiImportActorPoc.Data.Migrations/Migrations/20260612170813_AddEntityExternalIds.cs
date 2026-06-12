using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiImportActorPoc.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityExternalIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntityExternalIds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    System = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    EntityKind = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    InternalEntityId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityExternalIds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityExternalIds_InternalEntityId",
                table: "EntityExternalIds",
                column: "InternalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityExternalIds_System_Value",
                table: "EntityExternalIds",
                columns: new[] { "System", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityExternalIds");
        }
    }
}
