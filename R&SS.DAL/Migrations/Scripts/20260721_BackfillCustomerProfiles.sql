SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO Customers
    (
        UserId,
        CustomerCode,
        FullName,
        Phone,
        Email,
        Address,
        Notes,
        IsActive,
        CreatedAt,
        UpdatedAt
    )
    SELECT
        u.UserId,
        u.Username,
        u.FullName,
        u.Phone,
        u.Email,
        u.Address,
        NULL,
        u.IsActive,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    FROM Users AS u
    INNER JOIN UserRoles AS ur ON ur.UserId = u.UserId
    INNER JOIN Roles AS r ON r.RoleId = ur.RoleId
    WHERE r.RoleName = 'Customer'
      AND NOT EXISTS
      (
          SELECT 1
          FROM Customers AS c
          WHERE c.UserId = u.UserId
      )
      AND NOT EXISTS
      (
          SELECT 1
          FROM Customers AS c
          WHERE c.CustomerCode = u.Username
      );

    UPDATE c
    SET
        c.UserId = u.UserId,
        c.FullName = u.FullName,
        c.Phone = u.Phone,
        c.Email = u.Email,
        c.Address = u.Address,
        c.IsActive = u.IsActive,
        c.UpdatedAt = SYSUTCDATETIME()
    FROM Customers AS c
    INNER JOIN Users AS u ON u.Username = c.CustomerCode
    INNER JOIN UserRoles AS ur ON ur.UserId = u.UserId
    INNER JOIN Roles AS r ON r.RoleId = ur.RoleId
    WHERE r.RoleName = 'Customer'
      AND c.UserId IS NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM Customers AS existing
          WHERE existing.UserId = u.UserId
      );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
