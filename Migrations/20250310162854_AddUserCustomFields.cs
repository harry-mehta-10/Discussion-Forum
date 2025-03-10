using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscussionForum.Migrations
{
    public partial class AddUserCustomFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Name')
                BEGIN
                    ALTER TABLE AspNetUsers ADD Name nvarchar(max) NULL;
                END
                
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Location')
                BEGIN
                    ALTER TABLE AspNetUsers ADD Location nvarchar(max) NULL;
                END
                
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'ImageFilename')
                BEGIN
                    ALTER TABLE AspNetUsers ADD ImageFilename nvarchar(max) NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Name')
                BEGIN
                    ALTER TABLE AspNetUsers DROP COLUMN Name;
                END
                
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Location')
                BEGIN
                    ALTER TABLE AspNetUsers DROP COLUMN Location;
                END
                
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'ImageFilename')
                BEGIN
                    ALTER TABLE AspNetUsers DROP COLUMN ImageFilename;
                END
            ");
        }
    }
}