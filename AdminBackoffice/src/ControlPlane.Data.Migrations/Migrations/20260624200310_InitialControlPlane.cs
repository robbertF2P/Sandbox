using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlPlane.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialControlPlane : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DataTier = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LegacyBuildProfileId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LegacyRuntimeUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    LegacyDatabaseConnectionRef = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NativeDatabaseConnectionRef = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NativeApiBaseUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IntegrationPacksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomizationPacksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MigrationPhase = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MigrationTargetMode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    LastExportAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CutoverAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BillingTier = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SeatLimit = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProvisionedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastSyncedToPlatformAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastPlatformSyncError = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
