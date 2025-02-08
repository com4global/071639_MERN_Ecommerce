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
    o.OrderDate BETWEEN '2024-01-22' AND '2024-01-30' -- Filter by date range
    AND o.Status = 'completed' -- Only consider completed orders
GROUP BY 
    CONVERT(VARCHAR, o.OrderDate, 103), 
    p.Category
ORDER BY 
    [Date], 
    [Category];
