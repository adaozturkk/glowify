using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowify.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientsListToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IngredientsList",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IngredientsList",
                table: "Products");
        }
    }
}
