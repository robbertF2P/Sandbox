using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlatformConfig.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialPlatformConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "platform_config");

            migrationBuilder.CreateTable(
                name: "tenant_configurations",
                schema: "platform_config",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_configurations", x => x.TenantId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_configurations_Slug",
                schema: "platform_config",
                table: "tenant_configurations",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_configurations",
                schema: "platform_config");
        }
    }
}
