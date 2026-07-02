using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adopta.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthoredContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                schema: "adopta",
                table: "AuthoredContentItems",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Tooltip");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                schema: "adopta",
                table: "AuthoredContentItems");
        }
    }
}
