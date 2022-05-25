IF OBJECT_ID('[dbo].[ufnGetCustomerInformation]') IS NOT NULL
	DROP FUNCTION [dbo].[ufnGetCustomerInformation]
GO

CREATE FUNCTION [dbo].[ufnGetCustomerInformation](@CustomerID int)
RETURNS TABLE 
AS 
-- Returns the CustomerID, first name, and last name for the specified customer.
RETURN (
    SELECT 
        CustomerID, 
        FirstName, 
        LastName
    FROM [SalesLT].[Customer] 
    WHERE [CustomerID] = @CustomerID
);

GO

IF OBJECT_ID('[dbo].[ufnGetCustomerInformation]') IS NOT NULL
	PRINT '<<< CREATED FUNCTION [dbo].[ufnGetCustomerInformation] >>>'
ELSE
	PRINT '<<< FAILED CREATING FUNCTION [dbo].[ufnGetCustomerInformation] >>>'
GO

