-- Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Products Table
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Image NVARCHAR(255),
    Category NVARCHAR(50),
    NewPrice DECIMAL(10, 2) NOT NULL,
    OldPrice DECIMAL(10, 2),
    Available BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Orders Table
CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    OrderDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending',
    TotalAmount DECIMAL(10, 2) NOT NULL
);

-- OrderItems Table
CREATE TABLE OrderItems (
    OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL
);

-- Cart Table
CREATE TABLE Cart (
    CartId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL,
    AddedAt DATETIME DEFAULT GETDATE()
);
