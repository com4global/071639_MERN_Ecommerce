UPDATE Products
SET Size = 'L' -- Replace 'DefaultSize' with your desired default value
WHERE Size IS NULL;


ALTER TABLE Products
ALTER COLUMN Size SET NOT NULL;



-- Ensure no NULL values exist
UPDATE Products
SET Size = 'L'
WHERE Size IS NULL;

UPDATE Products
SET Description = 'Default Description'
WHERE Description IS NULL;

UPDATE Products
SET Image = 'default_image_url'
WHERE Image IS NULL;

UPDATE Products
SET Category = 'Default Category'
WHERE Category IS NULL;

UPDATE Products
SET Available = 1 -- Assuming Available is a BIT field (1 for true, 0 for false)
WHERE Available IS NULL;


ALTER TABLE Products
ALTER COLUMN Size NVARCHAR(255) NOT NULL;

ALTER TABLE Products
ALTER COLUMN Description NVARCHAR(MAX) NOT NULL;

ALTER TABLE Products
ALTER COLUMN Image NVARCHAR(255) NOT NULL;

ALTER TABLE Products
ALTER COLUMN Category NVARCHAR(255) NOT NULL;

ALTER TABLE Products
ALTER COLUMN Available BIT NOT NULL;