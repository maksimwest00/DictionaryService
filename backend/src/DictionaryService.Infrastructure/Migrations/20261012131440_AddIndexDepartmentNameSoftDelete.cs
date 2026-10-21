using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DictionaryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexDepartmentNameSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_departments_name_lower_trgm;");

            migrationBuilder.Sql(@"
                CREATE INDEX ix_departments_name_lower_trgm 
                ON departments USING gin (LOWER(name) gin_trgm_ops)
                WHERE is_active = true;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_departments_name_lower_trgm;");
        }
    }
}
