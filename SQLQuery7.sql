SELECT 
    CONVERT(VARCHAR, o.OrderDate, 103) AS [Date], -- Format the date as DD/MM/YYYY
    p.Category AS [Category],
    SUM(oi.Quantity) AS [TotalSellCount]
FROM 
    Orders o
JOIN 
    OrderItems oi ON o.OrderId = oi.OrderId
JOIN 
    Products p ON oi.ProductId = p.ProductId
WHERE 
    o.OrderDate BETWEEN '2024-01-22' AND '2024-01-30' -- Adjust date range as needed
GROUP BY 
    CONVERT(VARCHAR, o.OrderDate, 103), 
    p.Category
ORDER BY 
    [Date], 
    [Category];


    SELECT 
    CONVERT(VARCHAR, c.Date, 103) AS [Date], -- Format the date as DD/MM/YYYY
    p.Category AS [Category],
    SUM(c.Quantity) AS [TotalSellCount]
FROM 
    Cart c
JOIN 
    Products p ON c.ProductId = p.ProductId
WHERE 
    c.Date BETWEEN '2024-01-22' AND '2024-01-30' -- Adjust date range as needed
GROUP BY 
    CONVERT(VARCHAR, c.Date, 103), 
    p.Category
ORDER BY 
    [Date], 
    [Category];

