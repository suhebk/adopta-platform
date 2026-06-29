using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adopta.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAdoptaSchemaBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "adopta");

            migrationBuilder.CreateTable(
                name: "AdoptionUsers",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalUserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdoptionUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthoredContentItems",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthoredContentItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "adopta",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditEvents",
                schema: "adopta",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FailureCategory = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditEvents", x => new { x.TenantId, x.OccurredAtUtc, x.Action, x.Outcome });
                });

            migrationBuilder.CreateTable(
                name: "TenantApplications",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AllowedOrigin = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantMappings",
                schema: "adopta",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalTenantId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMappings", x => new { x.TenantId, x.ExternalTenantId, x.ApplicationId });
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrimaryDomain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DataRegion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticatedUserMappings",
                schema: "adopta",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalSubjectId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticatedUserMappings", x => new { x.TenantId, x.ExternalSubjectId });
                    table.ForeignKey(
                        name: "FK_AuthenticatedUserMappings_AdoptionUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "adopta",
                        principalTable: "AdoptionUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuthoredContentVersions",
                schema: "adopta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LifecycleState = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthoredContentVersions", x => new { x.ContentId, x.Id });
                    table.ForeignKey(
                        name: "FK_AuthoredContentVersions_AuthoredContentItems_ContentId",
                        column: x => x.ContentId,
                        principalSchema: "adopta",
                        principalTable: "AuthoredContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdoptionUserRoles",
                schema: "adopta",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdoptionUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AdoptionUserRoles_AdoptionUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "adopta",
                        principalTable: "AdoptionUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdoptionUserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "adopta",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "adopta",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionKey = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionKey });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionKey",
                        column: x => x.PermissionKey,
                        principalSchema: "adopta",
                        principalTable: "Permissions",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "adopta",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionUserRoles_RoleId",
                schema: "adopta",
                table: "AdoptionUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionUserRoles_TenantId_UserId_RoleId",
                schema: "adopta",
                table: "AdoptionUserRoles",
                columns: new[] { "TenantId", "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionUsers_TenantId_ExternalUserId",
                schema: "adopta",
                table: "AdoptionUsers",
                columns: new[] { "TenantId", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionUsers_TenantId_Id",
                schema: "adopta",
                table: "AdoptionUsers",
                columns: new[] { "TenantId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TenantId_Id",
                schema: "adopta",
                table: "AuditEvents",
                columns: new[] { "TenantId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TenantId_OccurredAtUtc",
                schema: "adopta",
                table: "AuditEvents",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticatedUserMappings_TenantId_ExternalSubjectId",
                schema: "adopta",
                table: "AuthenticatedUserMappings",
                columns: new[] { "TenantId", "ExternalSubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticatedUserMappings_TenantId_UserId",
                schema: "adopta",
                table: "AuthenticatedUserMappings",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticatedUserMappings_UserId",
                schema: "adopta",
                table: "AuthenticatedUserMappings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentItems_TenantId_ApplicationId_ContentKey",
                schema: "adopta",
                table: "AuthoredContentItems",
                columns: new[] { "TenantId", "ApplicationId", "ContentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentItems_TenantId_Id",
                schema: "adopta",
                table: "AuthoredContentItems",
                columns: new[] { "TenantId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentVersions_ContentId_Version",
                schema: "adopta",
                table: "AuthoredContentVersions",
                columns: new[] { "ContentId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthoredContentVersions_TenantId_ContentId",
                schema: "adopta",
                table: "AuthoredContentVersions",
                columns: new[] { "TenantId", "ContentId" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionKey",
                schema: "adopta",
                table: "RolePermissions",
                column: "PermissionKey");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_TenantId_RoleId_PermissionKey",
                schema: "adopta",
                table: "RolePermissions",
                columns: new[] { "TenantId", "RoleId", "PermissionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Id",
                schema: "adopta",
                table: "Roles",
                columns: new[] { "TenantId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                schema: "adopta",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEvents_TenantId_OccurredAtUtc",
                schema: "adopta",
                table: "SecurityAuditEvents",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantApplications_TenantId_Id",
                schema: "adopta",
                table: "TenantApplications",
                columns: new[] { "TenantId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantApplications_TenantId_Name",
                schema: "adopta",
                table: "TenantApplications",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantMappings_TenantId_ExternalTenantId_ApplicationId",
                schema: "adopta",
                table: "TenantMappings",
                columns: new[] { "TenantId", "ExternalTenantId", "ApplicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PrimaryDomain",
                schema: "adopta",
                table: "Tenants",
                column: "PrimaryDomain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdoptionUserRoles",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "AuditEvents",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "AuthenticatedUserMappings",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "AuthoredContentVersions",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "SecurityAuditEvents",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "TenantApplications",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "TenantMappings",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "AdoptionUsers",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "AuthoredContentItems",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "adopta");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "adopta");
        }
    }
}
