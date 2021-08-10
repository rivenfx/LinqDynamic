using Microsoft.EntityFrameworkCore.Migrations;

namespace TestApp.Migrations
{
    public partial class AddNewColumnDes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Desc",
                table: "Users");
        }
    }
}
