USE [anikafashionhouse2]
GO

/****** Object: SqlProcedure [dbo].[GetMostRecentProducts] Script Date: 12/23/2024 1:09:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
