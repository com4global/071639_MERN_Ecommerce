SELECT 
    CONVERT(VARCHAR, o.OrderDate, 103) AS [Date], -- Format as DD/MM/YYYY
    p.Category AS [Category],
    SUM(oi.Quantity) AS [TotalSalesCount]
FROM 
    OrderItems oi
JOIN 
    Orders o ON oi.OrderId = o.OrderId
JOIN 
    Products p ON oi.ProductId = p.ProductId
WHERE 
    o.OrderDate BETWEEN '2024-01-22' AND '2024-12-30' -- Filter by date range
    AND o.Status = 'completed' -- Only consider completed orders
GROUP BY 
    CONVERT(VARCHAR, o.OrderDate, 103), 
    p.Category
ORDER BY 
    [Date], 
    [Category];


    SELECT 
    TABLE_SCHEMA AS SchemaName,
    TABLE_NAME AS TableName,
    COLUMN_NAME AS ColumnName,
    DATA_TYPE AS DataType,
    CHARACTER_MAXIMUM_LENGTH AS MaxLength,
    IS_NULLABLE AS IsNullable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Products'
ORDER BY ORDINAL_POSITION;

go 
USE anikafashionhouse2;
go

ALTER PROCEDURE GetMostRecentProducts
AS
BEGIN
    -- Select top 5 most recent products from the Products table
    SELECT TOP 5 
        ProductId,
        Name,
        Image,
        Category,
        NewPrice,
        OldPrice,
        Available,
        Date,
        Size,
        Description
    FROM anikafashionhouse2.dbo.Products
    WHERE Date <= GETDATE() -- Ensure that products are from today
    ORDER BY Date DESC; -- Order by the most recent date
END


EXEC GetMostRecentProducts;
SELECT TOP 5 
        ProductId,
        Name,
        Image,
        Category,
        NewPrice,
        OldPrice,
        Available,
        Date,
        Size,
        Description
    FROM Products
    WHERE Date <= GETDATE() -- Ensure that products are from today
    ORDER BY Date DESC;

