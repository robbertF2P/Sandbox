using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiImportActorPoc.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentIsTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "Components",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Components");
        }
    }
}
