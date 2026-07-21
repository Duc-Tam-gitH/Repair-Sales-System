SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO Employees
    (
        UserId,
        RoleId,
        EmployeeCode,
        FullName,
        Email,
        Phone,
        Specialization,
        WorkStatus,
        IsActive,
        CreatedAt,
        UpdatedAt
    )
    SELECT
        u.UserId,
        r.RoleId,
        u.Username,
        u.FullName,
        u.Email,
        u.Phone,
        NULL,
        'Working',
        u.IsActive,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    FROM Users AS u
    INNER JOIN UserRoles AS ur ON ur.UserId = u.UserId
    INNER JOIN Roles AS r ON r.RoleId = ur.RoleId
    WHERE r.RoleName <> 'Customer'
      AND NOT EXISTS
      (
          SELECT 1
          FROM Employees AS e
          WHERE e.UserId = u.UserId
      )
      AND NOT EXISTS
      (
          SELECT 1
          FROM Employees AS e
          WHERE e.EmployeeCode = u.Username
      );

    UPDATE e
    SET
        e.RoleId = r.RoleId,
        e.EmployeeCode = u.Username,
        e.FullName = u.FullName,
        e.Email = u.Email,
        e.Phone = u.Phone,
        e.IsActive = u.IsActive,
        e.UpdatedAt = SYSUTCDATETIME()
    FROM Employees AS e
    INNER JOIN Users AS u ON u.UserId = e.UserId
    INNER JOIN UserRoles AS ur ON ur.UserId = u.UserId
    INNER JOIN Roles AS r ON r.RoleId = ur.RoleId
    WHERE r.RoleName <> 'Customer';

    DELETE c
    FROM Customers AS c
    INNER JOIN Users AS u ON u.UserId = c.UserId
    INNER JOIN UserRoles AS ur ON ur.UserId = u.UserId
    INNER JOIN Roles AS r ON r.RoleId = ur.RoleId
    WHERE r.RoleName <> 'Customer'
      AND NOT EXISTS (SELECT 1 FROM SalesOrders AS so WHERE so.CustomerId = c.CustomerId)
      AND NOT EXISTS (SELECT 1 FROM RepairOrders AS ro WHERE ro.CustomerId = c.CustomerId)
      AND NOT EXISTS (SELECT 1 FROM Payments AS p WHERE p.CustomerId = c.CustomerId)
      AND NOT EXISTS (SELECT 1 FROM ServiceRequests AS sr WHERE sr.CustomerId = c.CustomerId);

    DELETE e
    FROM Employees AS e
    INNER JOIN Users AS u ON u.UserId = e.UserId
    INNER JOIN UserRoles AS ur ON ur.UserId = u.UserId
    INNER JOIN Roles AS r ON r.RoleId = ur.RoleId
    WHERE r.RoleName = 'Customer';

    COMMIT TRANSACTION;

    SELECT
        c.CustomerId,
        c.UserId,
        u.Username,
        'Customer profile still has operational records and was not deleted.' AS Warning
    FROM Customers AS c
    INNER JOIN Users AS u ON u.UserId = c.UserId
    INNER JOIN UserRoles AS ur ON ur.UserId = u.UserId
    INNER JOIN Roles AS r ON r.RoleId = ur.RoleId
    WHERE r.RoleName <> 'Customer';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
