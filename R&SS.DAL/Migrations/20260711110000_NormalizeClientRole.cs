using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace R_SS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeClientRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @ClientRoleId int = (
                    SELECT TOP (1) [RoleId]
                    FROM [Roles]
                    WHERE [RoleName] = N'Client'
                );

                DECLARE @CustomerRoleId int = (
                    SELECT TOP (1) [RoleId]
                    FROM [Roles]
                    WHERE [RoleName] = N'Customer'
                );

                IF @ClientRoleId IS NULL AND @CustomerRoleId IS NULL
                BEGIN
                    INSERT INTO [Roles] ([RoleName], [Description])
                    VALUES (N'Customer', N'Default customer role.');
                END
                ELSE IF @ClientRoleId IS NOT NULL AND @CustomerRoleId IS NULL
                BEGIN
                    UPDATE [Roles]
                    SET [RoleName] = N'Customer',
                        [Description] = COALESCE([Description], N'Default customer role.')
                    WHERE [RoleId] = @ClientRoleId;
                END
                ELSE IF @ClientRoleId IS NOT NULL AND @CustomerRoleId IS NOT NULL AND @ClientRoleId <> @CustomerRoleId
                BEGIN
                    UPDATE [UserRoles]
                    SET [RoleId] = @CustomerRoleId
                    WHERE [RoleId] = @ClientRoleId
                      AND NOT EXISTS (
                          SELECT 1
                          FROM [UserRoles] AS [ExistingUserRole]
                          WHERE [ExistingUserRole].[UserId] = [UserRoles].[UserId]
                            AND [ExistingUserRole].[RoleId] = @CustomerRoleId
                      );

                    DELETE FROM [UserRoles]
                    WHERE [RoleId] = @ClientRoleId;

                    UPDATE [RolePermissions]
                    SET [RoleId] = @CustomerRoleId
                    WHERE [RoleId] = @ClientRoleId
                      AND NOT EXISTS (
                          SELECT 1
                          FROM [RolePermissions] AS [ExistingRolePermission]
                          WHERE [ExistingRolePermission].[UseCaseId] = [RolePermissions].[UseCaseId]
                            AND [ExistingRolePermission].[RoleId] = @CustomerRoleId
                      );

                    DELETE FROM [RolePermissions]
                    WHERE [RoleId] = @ClientRoleId;

                    DELETE FROM [Roles]
                    WHERE [RoleId] = @ClientRoleId;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
