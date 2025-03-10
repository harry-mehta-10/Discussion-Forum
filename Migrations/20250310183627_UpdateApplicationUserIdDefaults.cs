using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionForum.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUserIdDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set ApplicationUserId to NULL for all existing discussions
            migrationBuilder.Sql("UPDATE dbo.Discussions SET ApplicationUserId = NULL WHERE ApplicationUserId IS NULL;");

            // Set ApplicationUserId to NULL for all existing comments
            migrationBuilder.Sql("UPDATE dbo.Comments SET ApplicationUserId = NULL WHERE ApplicationUserId IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}