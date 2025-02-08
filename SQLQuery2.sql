ALTER TABLE Users
ADD Role NVARCHAR(255) NOT NULL ;


ALTER TABLE Cart
ADD CONSTRAINT FK_Cart_UserId FOREIGN KEY (UserId) REFERENCES Users(UserId);
USE anikafashionhouse2;
select * from OrderItems;
select * from Orders;

SELECT TOP 5 
    p.ProductId,
    p.Name,
    p.Image,
    p.NewPrice,
    p.OldPrice,
    p.Category,
    p.Available,
    p.Size,
    p.Date,
    p.Description,
    SUM(oi.Quantity) AS TotalSold
FROM 
    OrderItems oi
JOIN 
    Products p ON oi.ProductId = p.ProductId
JOIN 
    Orders o ON oi.OrderId = o.OrderId
WHERE 
    o.Status = 'Completed'
    AND p.Image IS NOT NULL
GROUP BY 
    p.ProductId,
    p.Name, 
    p.Image,
    p.NewPrice,
    p.OldPrice,
    p.Available,
    p.Size,
    p.Date,
    p.Description,
    p.Category
ORDER BY 
    p.Category, TotalSold DESC


    select * from Orders;
select * from OrderItems;
select * from Products;

SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ProductImages';

select * from ProductImages;

