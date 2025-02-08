select * from ProductImages;


SELECT 
                p.ProductId,
                p.Name,
                p.Image,
                p.Category,
                p.NewPrice,
                p.OldPrice,
                p.Available,
                p.Date,
                p.Size,
                p.Description,
                pi.ProductImageId,
                pi.ImageType,
                pi.ImageUrl
                
            FROM
                dbo.Products p
            LEFT JOIN
                dbo.ProductImages pi
            ON
                p.ProductId = pi.ProductId
            WHERE
                p.Category = 'kid';


WITH HoverImages AS (
    SELECT 
        pi.ProductImageId,
        pi.ProductId,
        pi.ImageUrl,
        ROW_NUMBER() OVER (PARTITION BY pi.ProductId ORDER BY pi.ProductImageId ASC) AS RowNum
    FROM
        dbo.ProductImages pi
    WHERE
        pi.ImageType = 'hover' -- Filter for hover images only
),
PrimaryImages AS (
    SELECT 
        pi.ProductImageId,
        pi.ProductId,
        pi.ImageUrl
    FROM
        dbo.ProductImages pi
    WHERE
        pi.ImageType = 'primary' -- Filter for primary images only
)
SELECT 
    p.ProductId,
    p.Name,
    p.Category,
    p.NewPrice,
    p.OldPrice,
    p.Available,
    p.Date,
    p.Size,
    p.Description,
    pri.ImageUrl AS PrimaryImageUrl,
    hi.ImageUrl AS HoverImageUrl
FROM
    dbo.Products p
LEFT JOIN
    PrimaryImages pri
ON
    p.ProductId = pri.ProductId
LEFT JOIN
    HoverImages hi
ON
    p.ProductId = hi.ProductId AND hi.RowNum = 1 -- Only the first hover image
WHERE
    p.Category = 'kid';


SELECT 
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
    MAX(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END) AS HoverImageUrl
FROM
    dbo.Products p
LEFT JOIN
    dbo.ProductImages pi
ON
    p.ProductId = pi.ProductId
WHERE
    p.Category = 'kid'
GROUP BY 
    p.ProductId, p.Name, p.Category, p.NewPrice, p.OldPrice, p.Available, 
    p.Date, p.Size, p.Description
HAVING
    MAX(CASE WHEN pi.ImageType = 'primary' THEN pi.ImageUrl END) IS NOT NULL
    OR MAX(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END) IS NOT NULL;


  SELECT 
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
    -- Format hover images as an array (e.g., ["image1", "image2"])
    CONCAT('[', STRING_AGG(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END, ','), ']') AS HoverImageUrls
FROM
    dbo.Products p
LEFT JOIN
    dbo.ProductImages pi
ON
    p.ProductId = pi.ProductId
GROUP BY 
    p.ProductId, p.Name, p.Category, p.NewPrice, p.OldPrice, p.Available, 
    p.Date, p.Size, p.Description
HAVING
    MAX(CASE WHEN pi.ImageType = 'primary' THEN pi.ImageUrl END) IS NOT NULL
    OR COUNT(CASE WHEN pi.ImageType = 'hover' THEN 1 END) > 0;
select * from ProductImages


SELECT 
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
    -- Use STRING_AGG to aggregate all hover images
    CONCAT('[', STRING_AGG(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END, ','), ']') AS HoverImageUrls
FROM
    dbo.Products p
LEFT JOIN
    dbo.ProductImages pi
ON
    p.ProductId = pi.ProductId
GROUP BY 
    p.ProductId, p.Name, p.Category, p.NewPrice, p.OldPrice, p.Available, 
    p.Date, p.Size, p.Description
HAVING
    MAX(CASE WHEN pi.ImageType = 'primary' THEN pi.ImageUrl END) IS NOT NULL
    -- Only filter for hover images if there's at least one hover image
    OR COUNT(CASE WHEN pi.ImageType = 'hover' THEN 1 END) > 0;
