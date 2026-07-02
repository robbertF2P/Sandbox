using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HourApprovals.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialHourApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hour_approvals");

            migrationBuilder.CreateTable(
                name: "active_tasks",
                schema: "hour_approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ActivityCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    HoursToGo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PlannedStart = table.Column<DateOnly>(type: "date", nullable: true),
                    PlannedFinish = table.Column<DateOnly>(type: "date", nullable: true),
                    AssignedUser = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_active_tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "approval_records",
                schema: "hour_approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalDay = table.Column<DateOnly>(type: "date", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    HoursToGo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PlannedStart = table.Column<DateOnly>(type: "date", nullable: true),
                    PlannedFinish = table.Column<DateOnly>(type: "date", nullable: true),
                    AssignedUser = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approval_records_active_tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "hour_approvals",
                        principalTable: "active_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_approval_records_TaskId_ApprovalDay",
                schema: "hour_approvals",
                table: "approval_records",
                columns: new[] { "TaskId", "ApprovalDay" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_records",
                schema: "hour_approvals");

            migrationBuilder.DropTable(
                name: "active_tasks",
                schema: "hour_approvals");
        }
    }
}
