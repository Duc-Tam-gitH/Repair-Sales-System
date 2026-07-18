using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace R_SS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class LinkUsersToCustomerAndEmployeeProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WorkStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Working"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                UPDATE [Customers]
                SET [UserId] = [Users].[UserId]
                FROM [Customers]
                INNER JOIN [Users] ON [Users].[Username] = [Customers].[CustomerCode]
                INNER JOIN [UserRoles] ON [UserRoles].[UserId] = [Users].[UserId]
                INNER JOIN [Roles] ON [Roles].[RoleId] = [UserRoles].[RoleId]
                WHERE [Roles].[RoleName] = N'Customer'
                  AND [Customers].[UserId] IS NULL;
                """);

            migrationBuilder.Sql("""
                INSERT INTO [Employees] (
                    [UserId],
                    [RoleId],
                    [EmployeeCode],
                    [FullName],
                    [Email],
                    [Phone],
                    [Specialization],
                    [WorkStatus],
                    [IsActive],
                    [CreatedAt],
                    [UpdatedAt]
                )
                SELECT
                    [Users].[UserId],
                    [Roles].[RoleId],
                    [Users].[Username],
                    [Users].[FullName],
                    [Users].[Email],
                    [Users].[Phone],
                    [Users].[Specialization],
                    [Users].[WorkStatus],
                    [Users].[IsActive],
                    GETDATE(),
                    GETDATE()
                FROM [Users]
                INNER JOIN [UserRoles] ON [UserRoles].[UserId] = [Users].[UserId]
                INNER JOIN [Roles] ON [Roles].[RoleId] = [UserRoles].[RoleId]
                WHERE [Roles].[RoleName] IN (N'Admin', N'Manager', N'Receptionist', N'Technician')
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [Employees]
                      WHERE [Employees].[UserId] = [Users].[UserId]
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_RoleId",
                table: "Employees",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserId",
                table: "Employees",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WorkStatus",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkStatus",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Working");

            migrationBuilder.Sql("""
                UPDATE [Users]
                SET
                    [Specialization] = [Employees].[Specialization],
                    [WorkStatus] = [Employees].[WorkStatus]
                FROM [Users]
                INNER JOIN [Employees] ON [Employees].[UserId] = [Users].[UserId];
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_UserId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customers");
        }
    }
}
