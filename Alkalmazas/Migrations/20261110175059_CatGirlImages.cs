using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alkalmazas.Migrations
{
    /// <inheritdoc />
    public partial class CatGirlImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "CatGirls",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "CatGirls");
        }
    }
}
