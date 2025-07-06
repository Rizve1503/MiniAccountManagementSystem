CREATE PROCEDURE sp_ManageBanks
    @Action NVARCHAR(10),
    @Id INT = NULL,
    @BankName NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'GETALL'
    BEGIN
        SELECT Id, BankName FROM dbo.Banks ORDER BY BankName;
    END

    ELSE IF @Action = 'GETBYID'
    BEGIN
        SELECT Id, BankName FROM dbo.Banks WHERE Id = @Id;
    END

    ELSE IF @Action = 'CREATE'
    BEGIN
        INSERT INTO dbo.Banks (BankName) VALUES (@BankName);
    END

    ELSE IF @Action = 'UPDATE'
    BEGIN
        UPDATE dbo.Banks SET BankName = @BankName WHERE Id = @Id;
    END

    ELSE IF @Action = 'DELETE'
    BEGIN
        DELETE FROM dbo.Banks WHERE Id = @Id;
    END
END