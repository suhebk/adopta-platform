using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adopta.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthoringHistoryPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthoredContentLifecycleHistory",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LifecycleAction = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FromState = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ToState = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthoredContentLifecycleHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthoredContentPublishingHistory",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthoredContentPublishingHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentLifecycleHistory_TenantId_ContentId_VersionId",
                schema: "adopta",
                table: "AuthoredContentLifecycleHistory",
                columns: new[] { "TenantId", "ContentId", "VersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentLifecycleHistory_TenantId_OccurredAtUtc",
                schema: "adopta",
                table: "AuthoredContentLifecycleHistory",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentPublishingHistory_TenantId_ContentId_VersionId",
                schema: "adopta",
                table: "AuthoredContentPublishingHistory",
                columns: new[] { "TenantId", "ContentId", "VersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentPublishingHistory_TenantId_Environment_Channel",
                schema: "adopta",
                table: "AuthoredContentPublishingHistory",
                columns: new[] { "TenantId", "Environment", "Channel" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentPublishingHistory_TenantId_OccurredAtUtc",
                schema: "adopta",
                table: "AuthoredContentPublishingHistory",
                columns: new[] { "TenantId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthoredContentLifecycleHistory",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "AuthoredContentPublishingHistory",
                schema: "adopta");
        }
    }
}
