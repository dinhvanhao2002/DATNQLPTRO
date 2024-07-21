using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccommodationSearchSystem.Migrations
{
    public partial class IsShowCancel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShowCancel",
                table: "Post",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShowCancel",
                table: "Post");
        }
    }
}
