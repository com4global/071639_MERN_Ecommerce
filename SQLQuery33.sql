 SELECT TOP 5 
        p.ProductId,
        p.Name,
        p.Category,
        p.NewPrice,
        p.OldPrice,
        p.Available,
        p.Date,
        p.Size,
        p.Description,
        MAX(CASE WHEN pi.ImageType = 'primary' THEN pi.ImageUrl END) AS PrimaryImageUrl,
        MAX(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END) AS HoverImageUrl,
        SUM(oi.Quantity) AS TotalSold
    FROM
        dbo.Products p
    LEFT JOIN
        dbo.ProductImages pi ON p.ProductId = pi.ProductId
    LEFT JOIN
        dbo.OrderItems oi ON p.ProductId = oi.ProductId
    LEFT JOIN
        dbo.Orders o ON oi.OrderId = o.OrderId
    WHERE
        
         (o.Status = 'Completed' OR o.Status IS NULL)
    GROUP BY 
        p.ProductId, p.Name, p.Category, p.NewPrice, p.OldPrice, 
        p.Available, p.Date, p.Size, p.Description
    HAVING
        MAX(CASE WHEN pi.ImageType = 'primary' THEN pi.ImageUrl END) IS NOT NULL
        OR MAX(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END) IS NOT NULL
    ORDER BY 
        TotalSold DESC