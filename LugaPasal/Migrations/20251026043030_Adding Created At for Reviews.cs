using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LugaPasal.Migrations
{
    /// <inheritdoc />
    public partial class AddingCreatedAtforReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CreatedAt",
                table: "Ratings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Ratings");
        }
    }
}
