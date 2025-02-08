




Use anikafashionhouse2;
GO




CREATE PROCEDURE SaveCheckoutData
    @UserId INT,
    @TotalAmount DECIMAL(10, 2),
    @DeliveryMethod NVARCHAR(50),
    @AddressLine1 NVARCHAR(255) = NULL,
    @AddressLine2 NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @PostalCode NVARCHAR(20) = NULL,
    @Country NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @MarketingConsent BIT = 0
AS
BEGIN
    -- Start transaction
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Insert into Orders table
        INSERT INTO Orders (UserId, TotalAmount, DeliveryMethod)
        VALUES (@UserId, @TotalAmount, @DeliveryMethod);

        DECLARE @OrderId INT = SCOPE_IDENTITY(); -- Get the generated OrderId

        -- If the delivery method is 'Ship', insert into ShippingAddresses
        IF (@DeliveryMethod = 'Ship')
        BEGIN
            INSERT INTO ShippingAddresses (OrderId, AddressLine1, AddressLine2, City, State, PostalCode, Country, Phone)
            VALUES (@OrderId, @AddressLine1, @AddressLine2, @City, @State, @PostalCode, @Country, @Phone);
        END

        -- Update MarketingConsent in Users table
        UPDATE Users
        SET MarketingConsent = @MarketingConsent
        WHERE UserId = @UserId;

        -- Commit transaction
        COMMIT TRANSACTION;
    END TRY

    BEGIN CATCH
        -- Rollback transaction in case of error
        ROLLBACK TRANSACTION;

        -- Rethrow the error for debugging
        THROW;
    END CATCH
END
GO

Use anikafashionhouse2;
GO

CREATE PROCEDURE InsertProduct
    @Name NVARCHAR(100),
    @Category NVARCHAR(255),
    @NewPrice DECIMAL,
    @OldPrice DECIMAL = NULL,
    @Available BIT,
    @Date DATETIME = NULL,
    @Size NVARCHAR(255),
    @Description NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Products (Name, Category, NewPrice, OldPrice, Available, Date, Size, Description)
    VALUES (@Name, @Category, @NewPrice, @OldPrice, @Available, @Date, @Size, @Description);
    
    -- Return the generated ProductId
    SELECT SCOPE_IDENTITY();
END;
GO
USE anikafashionhouse2;
go
-- Alter procedure in a separate batch
CREATE PROCEDURE SaveCheckoutData
    @UserId INT,
    @TotalAmount DECIMAL(10, 2),
    @DeliveryMethod NVARCHAR(50),
    @AddressLine1 NVARCHAR(255) = NULL,
    @AddressLine2 NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @PostalCode NVARCHAR(20) = NULL,
    @Country NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @MarketingConsent BIT = 0,
    @CartItems CartItemType READONLY
AS
BEGIN
    -- Start transaction
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Insert into Orders table (including default values for Status and OrderDate)
        INSERT INTO Orders (UserId, TotalAmount, DeliveryMethod, Status, OrderDate)
        VALUES (@UserId, @TotalAmount, @DeliveryMethod, 'Pending', GETDATE());

        DECLARE @OrderId INT = SCOPE_IDENTITY(); -- Get the generated OrderId

        -- If the delivery method is 'Ship', insert into ShippingAddresses
        IF (@DeliveryMethod = 'Ship')
        BEGIN
            INSERT INTO ShippingAddresses (OrderId, AddressLine1, AddressLine2, City, State, PostalCode, Country, Phone)
            VALUES (@OrderId, @AddressLine1, @AddressLine2, @City, @State, @PostalCode, @Country, @Phone);
        END

        -- Insert each CartItem into OrderItems table
        INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
        SELECT @OrderId, ProductId, Quantity, Price
        FROM @CartItems;

        -- Update MarketingConsent in Users table
        UPDATE Users
        SET MarketingConsent = @MarketingConsent
        WHERE UserId = @UserId;

        -- Commit transaction
        COMMIT TRANSACTION;
    END TRY

    BEGIN CATCH
        -- Rollback transaction in case of error
        ROLLBACK TRANSACTION;

        -- Rethrow the error for debugging
        THROW;
    END CATCH
END


select * from Orders;



GO
CREATE PROCEDURE InsertProductImage
    @ProductId INT,
    @ImageType VARCHAR(50),
    @ImageUrl VARCHAR(255)
AS
BEGIN
    INSERT INTO ProductImages (ProductId, ImageType, ImageUrl)
    VALUES (@ProductId, @ImageType, @ImageUrl);
END;

GO

CREATE PROCEDURE UpdateProductDetails
    @ProductId INT,
    @Name NVARCHAR(100),
    @Category NVARCHAR(255),
    @NewPrice DECIMAL,
    @OldPrice DECIMAL,
    @Available BIT,
    @Date DATETIME,
    @Size NVARCHAR(255),
    @Description NVARCHAR(MAX)
AS
BEGIN
    UPDATE Products
    SET 
        Name = @Name,
        Category = @Category,
        NewPrice = @NewPrice,
        OldPrice = @OldPrice,
        Available = @Available,
        Date = @Date,
        Size = @Size,
        Description = @Description
    WHERE ProductId = @ProductId
END

GO

CREATE PROCEDURE UpdateProductImages
    @ProductId INT,
    @PrimaryImageUrl VARCHAR(255),
    @HoverImageUrl VARCHAR(255)
AS
BEGIN
    -- Update primary image URL if provided
    IF @PrimaryImageUrl IS NOT NULL
    BEGIN
        UPDATE ProductImages
        SET ImageUrl = @PrimaryImageUrl
        WHERE ProductId = @ProductId AND ImageType = 'primary'
    END

    -- Update hover image URL if provided
    IF @HoverImageUrl IS NOT NULL
    BEGIN
        UPDATE ProductImages
        SET ImageUrl = @HoverImageUrl
        WHERE ProductId = @ProductId AND ImageType = 'hover'
    END
END

GO
CREATE PROCEDURE GetAllProducts
AS
BEGIN
    SELECT 
        p.ProductId, 
        p.Name, 
        piPrimary.ImageUrl AS PrimaryImage, 
        piHover.ImageUrl AS HoverImage,
        p.Category, 
        p.NewPrice, 
        p.OldPrice, 
        p.Available, 
        p.Date, 
        p.Size, 
        p.Description
    FROM 
        Products p
    LEFT JOIN ProductImages piPrimary ON p.ProductId = piPrimary.ProductId AND piPrimary.ImageType = 'primary'
    LEFT JOIN ProductImages piHover ON p.ProductId = piHover.ProductId AND piHover.ImageType = 'hover'
END

GO
CREATE PROCEDURE GetProductById
    @ProductId INT
AS
BEGIN
    BEGIN TRANSACTION
    BEGIN TRY
        SELECT 
            p.ProductId, 
            p.Name, 
            piPrimary.ImageUrl AS PrimaryImage, 
            piHover.ImageUrl AS HoverImage,
            p.Category, 
            p.NewPrice, 
            p.OldPrice, 
            p.Available, 
            p.Date, 
            p.Size, 
            p.Description
        FROM 
            Products p
        LEFT JOIN ProductImages piPrimary ON p.ProductId = piPrimary.ProductId AND piPrimary.ImageType = 'primary'
        LEFT JOIN ProductImages piHover ON p.ProductId = piHover.ProductId AND piHover.ImageType = 'hover'
        WHERE p.ProductId = @ProductId

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW;
    END CATCH
END

GO
SELECT OBJECT_ID('SaveCheckoutData'), OBJECT_ID('InsertProduct'), OBJECT_ID('InsertProductImage');
GO

Use anikafashionhouse2;
GO

ALTER PROCEDURE GetMostRecentProducts
AS
BEGIN
    BEGIN TRANSACTION
    BEGIN TRY
        -- Log that the process has started
        --INSERT INTO LogEntries (LogLevel, Message, LogDate)
        --VALUES ('INFO', 'Started fetching most recent products.', GETDATE());

        -- Fetch top 5 most recent products along with their images
        SELECT TOP 5
            p.ProductId,
            p.Name,
            piPrimary.ImageUrl AS PrimaryImage,
            piHover.ImageUrl AS HoverImage,
            p.Category,
            p.NewPrice,
            p.OldPrice,
            p.Available,
            p.Date,
            p.Size,
            p.Description
        FROM Products p
        LEFT JOIN ProductImages piPrimary ON p.ProductId = piPrimary.ProductId AND piPrimary.ImageType = 'primary'
        LEFT JOIN ProductImages piHover ON p.ProductId = piHover.ProductId AND piHover.ImageType = 'hover'
        WHERE p.Date <= GETDATE()
        ORDER BY p.Date DESC;

        -- Log success
        --INSERT INTO LogEntries (LogLevel, Message, LogDate)
        --VALUES ('INFO', 'Successfully fetched most recent products.', GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Rollback in case of any error
        ROLLBACK TRANSACTION;

        -- Log the error details
        --INSERT INTO LogEntries (LogLevel, Message, ExceptionMessage, LogDate)
       -- VALUES ('ERROR', 'Error occurred in GetMostRecentProducts.', ERROR_MESSAGE(), GETDATE());

        -- Rethrow the error to propagate it to the caller
        THROW;
    END CATCH
END
GO
CREATE PROCEDURE GetTopSoldItemsPerCategory
AS
BEGIN
    BEGIN TRANSACTION
    BEGIN TRY
        -- Fetch top 5 sold items per category with images
        SELECT TOP 5
            p.ProductId,
            p.Name,
            piPrimary.ImageUrl AS PrimaryImage,
            piHover.ImageUrl AS HoverImage,
            p.NewPrice,
            p.OldPrice,
            p.Category,
            p.Available,
            p.Size,
            p.Date,
            p.Description,
            SUM(oi.Quantity) AS TotalSold
        FROM OrderItems oi
        JOIN Products p ON oi.ProductId = p.ProductId
        JOIN Orders o ON oi.OrderId = o.OrderId
        LEFT JOIN ProductImages piPrimary ON p.ProductId = piPrimary.ProductId AND piPrimary.ImageType = 'primary'
        LEFT JOIN ProductImages piHover ON p.ProductId = piHover.ProductId AND piHover.ImageType = 'hover'
        WHERE o.Status = 'completed'
        GROUP BY 
            p.ProductId,
            p.Name,
            piPrimary.ImageUrl,
            piHover.ImageUrl,
            p.NewPrice,
            p.OldPrice,
            p.Available,
            p.Size,
            p.Date,
            p.Description,
            p.Category
        ORDER BY 
            p.Category, TotalSold DESC;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Rollback transaction on error
        ROLLBACK TRANSACTION;

        -- Log or handle error as needed
        THROW;
    END CATCH
END;


go 
EXEC GetMostRecentProducts;


ALTER TABLE [dbo].[anikafashionhouse2].[Products]
ADD Image NVARCHAR(255);


USE anikafashionhouse2
select * from [dbo].[anikafashionhouse2].[Orders];






