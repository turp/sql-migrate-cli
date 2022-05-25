IF OBJECT_ID('[SalesLT].[vProductAndDescription]') IS NOT NULL
	DROP VIEW [SalesLT].[vProductAndDescription]
GO


CREATE VIEW [SalesLT].[vProductAndDescription] 
WITH SCHEMABINDING 
AS 
-- View (indexed or standard) to display products and product descriptions by language.
SELECT 
    p.[ProductID] 
    ,p.[Name] 
    ,pm.[Name] AS [ProductModel] 
    ,pmx.[Culture] 
    ,pd.[Description] 
FROM [SalesLT].[Product] p 
    INNER JOIN [SalesLT].[ProductModel] pm 
    ON p.[ProductModelID] = pm.[ProductModelID] 
    INNER JOIN [SalesLT].[ProductModelProductDescription] pmx 
    ON pm.[ProductModelID] = pmx.[ProductModelID] 
    INNER JOIN [SalesLT].[ProductDescription] pd 
    ON pmx.[ProductDescriptionID] = pd.[ProductDescriptionID];

GO

IF OBJECT_ID('[SalesLT].[vProductAndDescription]') IS NOT NULL
	PRINT '<<< CREATED VIEW [SalesLT].[vProductAndDescription] >>>'
ELSE
	PRINT '<<< FAILED CREATING VIEW [SalesLT].[vProductAndDescription] >>>'
GO

