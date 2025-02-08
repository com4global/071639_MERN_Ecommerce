ALTER TABLE Orders
ADD DeliveryMethod NVARCHAR(50) DEFAULT 'Pickup'; -- Default to 'Pickup'

ALTER TABLE Users
ADD MarketingConsent BIT DEFAULT 0; -- Default to 'No Consent'



CREATE TABLE ShippingAddresses (
    ShippingAddressId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId),
    AddressLine1 NVARCHAR(255) NOT NULL,
    AddressLine2 NVARCHAR(255),
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    PostalCode NVARCHAR(20) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
