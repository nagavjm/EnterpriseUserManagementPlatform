using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseUserManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialAzureSqlCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Department", "Email", "FirstName", "IsActive", "LastName", "ModifiedDate", "Role" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "eleanor.vance@enterprise.com", "Eleanor", true, "Vance", null, "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "marcus.brody@enterprise.com", "Marcus", true, "Brody", null, "Manager" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engineering", "sophia.chen@enterprise.com", "Sophia", true, "Chen", null, "Employee" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HR", "david.miller@enterprise.com", "David", true, "Miller", null, "Manager" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HR", "rachel.green@enterprise.com", "Rachel", true, "Green", null, "Employee" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Finance", "alexander.hamilton@enterprise.com", "Alexander", true, "Hamilton", null, "Manager" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Department", "Email", "FirstName", "LastName", "ModifiedDate", "Role" },
                values: new object[] { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sales", "jordan.belfort@enterprise.com", "Jordan", "Belfort", new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Employee" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Department", "Email", "FirstName", "IsActive", "LastName", "ModifiedDate", "Role" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Operations", "amelia.earhart@enterprise.com", "Amelia", true, "Earhart", null, "Employee" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2025, 10, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Support", "lucas.scott@enterprise.com", "Lucas", true, "Scott", null, "Employee" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
