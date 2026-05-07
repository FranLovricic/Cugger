using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cugger.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmationAndUntappdFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "Users",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationTokenExpiresAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Beers",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Klasični Guinness Stout s karakterističnom tamnom bojom");

            migrationBuilder.UpdateData(
                table: "Beers",
                keyColumn: "Id",
                keyValue: 5,
                column: "Description",
                value: "Ekstremno hopna IPA s intenzivnom gorčinom");

            migrationBuilder.UpdateData(
                table: "Breweries",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Legendarni proizvođač Guinness piva");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Bio", "EmailConfirmationToken", "EmailConfirmationTokenExpiresAt", "IsEmailConfirmed" },
                values: new object[] { "Apsolvent pivarstva i ljubitelj kvalitetnih piva", null, null, true });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EmailConfirmationToken", "EmailConfirmationTokenExpiresAt", "IsEmailConfirmed" },
                values: new object[] { null, null, true });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EmailConfirmationToken", "EmailConfirmationTokenExpiresAt", "IsEmailConfirmed" },
                values: new object[] { null, null, true });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EmailConfirmationToken", "EmailConfirmationTokenExpiresAt", "IsEmailConfirmed" },
                values: new object[] { null, null, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Beers",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Klasični Guinness Stout sa karakterističnom tamnom bojom");

            migrationBuilder.UpdateData(
                table: "Beers",
                keyColumn: "Id",
                keyValue: 5,
                column: "Description",
                value: "Ekstremno hopno IPA s intenzivnom gorčinom");

            migrationBuilder.UpdateData(
                table: "Breweries",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Legendarni proizvođač Guinnessa");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Bio",
                value: "Apsolventist pivarstva i ljubitelj kvalitetnih piva");
        }
    }
}
