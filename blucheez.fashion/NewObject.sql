-- Employee Table
CREATE TABLE Employee (
    EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
    Position NVARCHAR(50),
    JoinDate DATE NOT NULL,
    DepartmentID INT,
    Status NVARCHAR(10) CHECK (Status IN ('active', 'inactive', 'deleted')) DEFAULT 'active',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- Department Table
CREATE TABLE Department (
    DepartmentID INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL,
    ManagerID INT NULL, -- Manager is an employee
    Budget DECIMAL(18,2) CHECK (Budget >= 0)
);

-- PerformanceReview Table
CREATE TABLE PerformanceReview (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeID INT NOT NULL,
    ReviewDate DATE NOT NULL,
    ReviewScore DECIMAL(4,2) CHECK (ReviewScore BETWEEN 0 AND 100),
    ReviewNotes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);


-- Index on Employee Table for common queries
CREATE INDEX IDX_Employee_Name ON Employee (Name);
CREATE INDEX IDX_Employee_Position ON Employee (Position);
CREATE INDEX IDX_Employee_Status ON Employee (Status);

-- Index on Department Table for DepartmentName
CREATE INDEX IDX_Department_Name ON Department (DepartmentName);

-- Index on PerformanceReview Table for EmployeeID and ReviewScore
CREATE INDEX IDX_PerformanceReview_EmployeeID ON PerformanceReview (EmployeeID);
CREATE INDEX IDX_PerformanceReview_Score ON PerformanceReview (ReviewScore);


UPDATE Employee
SET Status = 'deleted'
WHERE EmployeeID = @EmployeeID;



DECLARE @PageNumber INT = 1, @PageSize INT = 10;

SELECT EmployeeID, Name, Email, Position, JoinDate, Status
FROM Employee
WHERE Status = 'active'
ORDER BY EmployeeID
OFFSET (@PageNumber - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;


SELECT e.EmployeeID, e.Name, e.Email, e.Position, d.DepartmentName
FROM Employee e
INNER JOIN Department d ON e.DepartmentID = d.DepartmentID
WHERE d.DepartmentName = 'IT';


DECLARE @PageNumber INT = 1, @PageSize INT = 5;

SELECT pr.ReviewID, e.Name AS EmployeeName, pr.ReviewDate, pr.ReviewScore, pr.ReviewNotes
FROM PerformanceReview pr
INNER JOIN Employee e ON pr.EmployeeID = e.EmployeeID
ORDER BY pr.ReviewDate DESC
OFFSET (@PageNumber - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;

Use employeemanagementsystem;

GO

CREATE PROCEDURE AddEmployee
    @Name NVARCHAR(100), @Email NVARCHAR(100), @Phone NVARCHAR(20),
    @Position NVARCHAR(50), @JoinDate DATE, @DepartmentID INT
AS
BEGIN
    INSERT INTO [dbo].[employeemanagementsystem].[Employee] (Name, Email, Phone, Position, JoinDate, DepartmentID)
    VALUES (@Name, @Email, @Phone, @Position, @JoinDate, @DepartmentID);
END

Go

CREATE PROCEDURE GetEmployees
    @PageNumber INT, @PageSize INT
AS
BEGIN
    SELECT * FROM [dbo].[employeemanagementsystem].[Employee]
    WHERE Status != 'deleted'
    ORDER BY EmployeeID
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END

Go

CREATE PROCEDURE UpdateEmployee
    @EmployeeID INT, @Name NVARCHAR(100), @Email NVARCHAR(100), @Phone NVARCHAR(20),
    @Position NVARCHAR(50), @DepartmentID INT
AS
BEGIN
    UPDATE [dbo].[employeemanagementsystem].[Employee]
    SET Name = @Name, Email = @Email, Phone = @Phone, Position = @Position, DepartmentID = @DepartmentID
    WHERE EmployeeID = @EmployeeID;
END

Go

CREATE PROCEDURE DeleteEmployee
    @EmployeeID INT
AS
BEGIN
    UPDATE [dbo].[employeemanagementsystem].[Employee] SET Status = 'deleted' WHERE EmployeeID = @EmployeeID;
END


Go

CREATE PROCEDURE AddDepartment
    @DepartmentName NVARCHAR(100), @ManagerID INT, @Budget DECIMAL(18,2)
AS
BEGIN
    INSERT INTO [dbo].[employeemanagementsystem].[Department] (DepartmentName, ManagerID, Budget)
    VALUES (@DepartmentName, @ManagerID, @Budget);
END

Go

CREATE PROCEDURE GetDepartments
AS
BEGIN
    SELECT * FROM [dbo].[employeemanagementsystem].[Department]
END

Go

CREATE PROCEDURE AddPerformanceReview
    @EmployeeID INT, @ReviewDate DATE, @ReviewScore DECIMAL(4,2), @ReviewNotes NVARCHAR(500)
AS
BEGIN
    INSERT INTO[dbo].[employeemanagementsystem].[PerformanceReview] (EmployeeID, ReviewDate, ReviewScore, ReviewNotes)
    VALUES (@EmployeeID, @ReviewDate, @ReviewScore, @ReviewNotes);
END


Go

CREATE PROCEDURE GetDepartmentAverageScore
AS
BEGIN
    SELECT d.DepartmentName, AVG(pr.ReviewScore) AS AvgScore
    FROM [dbo].[employeemanagementsystem].[PerformanceReview pr]
    INNER JOIN Employee e ON pr.EmployeeID = e.EmployeeID
    INNER JOIN Department d ON e.DepartmentID = d.DepartmentID
    GROUP BY d.DepartmentName;
END













