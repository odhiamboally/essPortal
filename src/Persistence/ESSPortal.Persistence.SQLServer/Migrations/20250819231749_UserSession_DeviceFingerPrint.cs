using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESSPortal.Persistence.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class UserSession_DeviceFingerPrint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceFingerprint",
                table: "UserSessions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceFingerprint",
                table: "UserSessions");
        }
    }
}
