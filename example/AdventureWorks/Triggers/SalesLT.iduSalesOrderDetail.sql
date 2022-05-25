IF OBJECT_ID('[SalesLT].[iduSalesOrderDetail]') IS NOT NULL
	DROP TRIGGER [SalesLT].[iduSalesOrderDetail]
GO


CREATE TRIGGER [SalesLT].[iduSalesOrderDetail] ON [SalesLT].[SalesOrderDetail] 
AFTER INSERT, DELETE, UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- If inserting or updating these columns
        IF UPDATE([ProductID]) OR UPDATE([OrderQty]) OR UPDATE([UnitPrice]) OR UPDATE([UnitPriceDiscount]) 

        -- Update SubTotal in SalesOrderHeader record. Note that this causes the 
        -- SalesOrderHeader trigger to fire which will update the RevisionNumber.
        UPDATE [SalesLT].[SalesOrderHeader]
        SET [SalesLT].[SalesOrderHeader].[SubTotal] = 
            (SELECT SUM([SalesLT].[SalesOrderDetail].[LineTotal])
                FROM [SalesLT].[SalesOrderDetail]
                WHERE [SalesLT].[SalesOrderHeader].[SalesOrderID] = [SalesLT].[SalesOrderDetail].[SalesOrderID])
        WHERE [SalesLT].[SalesOrderHeader].[SalesOrderID] IN (SELECT inserted.[SalesOrderID] FROM inserted);

    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;

GO

IF OBJECT_ID('[SalesLT].[iduSalesOrderDetail]') IS NOT NULL
	PRINT '<<< CREATED TRIGGER [SalesLT].[iduSalesOrderDetail] >>>'
ELSE
	PRINT '<<< FAILED CREATING TRIGGER [SalesLT].[iduSalesOrderDetail] >>>'
GO

