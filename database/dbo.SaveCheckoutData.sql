-- Alter procedure in a separate batch
 Alter PROCEDURE SaveCheckoutData
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
        VALUES (@UserId, @TotalAmount, @DeliveryMethod, 'Completed', GETDATE());

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
select * from OrderItems;
select * from Products;