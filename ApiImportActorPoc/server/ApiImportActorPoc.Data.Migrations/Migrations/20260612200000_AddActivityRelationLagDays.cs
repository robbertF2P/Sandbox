using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiImportActorPoc.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityRelationLagDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LagDays",
                table: "ActivityRelations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LagDays",
                table: "ActivityRelations");
        }
    }
}
