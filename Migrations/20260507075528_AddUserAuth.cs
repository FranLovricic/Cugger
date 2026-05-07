using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cugger.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiresAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordSalt",
                table: "Users",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiresAt", "PasswordSalt" },
                values: new object[] { "SEED_NEEDS_HASH", null, null, "SEED_NEEDS_HASH" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiresAt", "PasswordSalt" },
                values: new object[] { "SEED_NEEDS_HASH", null, null, "SEED_NEEDS_HASH" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiresAt", "PasswordSalt" },
                values: new object[] { "SEED_NEEDS_HASH", null, null, "SEED_NEEDS_HASH" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiresAt", "PasswordSalt" },
                values: new object[] { "SEED_NEEDS_HASH", null, null, "SEED_NEEDS_HASH" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "Users");
        }
    }
}
