using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LugaPasal.Migrations
{
    /// <inheritdoc />
    public partial class Addingproductcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductCategory",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductCategory",
                table: "Products");
        }
    }
}
