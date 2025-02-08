ALTER TABLE Cart
ADD CONSTRAINT FK_Cart_UserId FOREIGN KEY (UserId) REFERENCES Users(UserId);

ALTER TABLE Cart DROP CONSTRAINT FK__Cart__UserId__34C8D9D1;








SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM 
    sys.foreign_keys AS fk
INNER JOIN 
    sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN 
    sys.tables AS tp ON fkc.parent_object_id = tp.object_id
INNER JOIN 
    sys.tables AS tr ON fkc.referenced_object_id = tr.object_id
INNER JOIN 
    sys.columns AS cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
INNER JOIN 
    sys.columns AS cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
WHERE 
    tp.name IN ('Cart', 'Orders', 'OrderItems');




