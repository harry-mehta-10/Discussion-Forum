using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionForum.Migrations
{
    public partial class DropIdentityTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys first
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_ApplicationUserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Discussions_AspNetUsers_ApplicationUserId",
                table: "Discussions");

            // Drop Identity tables
            migrationBuilder.Sql("DROP TABLE IF EXISTS [AspNetUserTokens];");
            migrationBuilder.Sql("DROP TABLE IF EXISTS [AspNetUserRoles];");
            migrationBuilder.Sql("DROP TABLE IF EXISTS [AspNetUserLogins];");
            migrationBuilder.Sql("DROP TABLE IF EXISTS [AspNetUserClaims];");
            migrationBuilder.Sql("DROP TABLE IF EXISTS [AspNetRoleClaims];");
            migrationBuilder.Sql("DROP TABLE IF EXISTS [AspNetUsers];");
            migrationBuilder.Sql("DROP TABLE IF EXISTS [AspNetRoles];");

            // Alter ApplicationUserId columns
            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Discussions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Re-add foreign keys if needed
            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_ApplicationUserId",
                table: "Comments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Discussions_AspNetUsers_ApplicationUserId",
                table: "Discussions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}