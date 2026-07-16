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
                    VALUES (N'Client', N'Default client role.');
                END
                ELSE IF @ClientRoleId IS NULL AND @CustomerRoleId IS NOT NULL
                BEGIN
                    UPDATE [Roles]
                    SET [RoleName] = N'Client',
                        [Description] = COALESCE([Description], N'Default client role.')
                    WHERE [RoleId] = @CustomerRoleId;
                END
                ELSE IF @ClientRoleId IS NOT NULL AND @CustomerRoleId IS NOT NULL AND @ClientRoleId <> @CustomerRoleId
                BEGIN
                    UPDATE [UserRoles]
                    SET [RoleId] = @ClientRoleId
                    WHERE [RoleId] = @CustomerRoleId
                      AND NOT EXISTS (
                          SELECT 1
                          FROM [UserRoles] AS [ExistingUserRole]
                          WHERE [ExistingUserRole].[UserId] = [UserRoles].[UserId]
                            AND [ExistingUserRole].[RoleId] = @ClientRoleId
                      );

                    DELETE FROM [UserRoles]
                    WHERE [RoleId] = @CustomerRoleId;

                    UPDATE [RolePermissions]
                    SET [RoleId] = @ClientRoleId
                    WHERE [RoleId] = @CustomerRoleId
                      AND NOT EXISTS (
                          SELECT 1
                          FROM [RolePermissions] AS [ExistingRolePermission]
                          WHERE [ExistingRolePermission].[UseCaseId] = [RolePermissions].[UseCaseId]
                            AND [ExistingRolePermission].[RoleId] = @ClientRoleId
                      );

                    DELETE FROM [RolePermissions]
                    WHERE [RoleId] = @CustomerRoleId;

                    DELETE FROM [Roles]
                    WHERE [RoleId] = @CustomerRoleId;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
