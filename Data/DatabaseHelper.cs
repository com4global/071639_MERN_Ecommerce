
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using ApiOnLamda.Model;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;
using Dapper;
using Google.Apis.Auth;
using log4net;

//using Microsoft.SqlServer.Dac.Model;
namespace ApiOnLamda.Data
{


    public class DatabaseHelper
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Example Method for Adding User
        public async Task<int> AddUser(User user)
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand("INSERT INTO Users (Name, Email, Password, Date,Role) VALUES (@Name, @Email, @Password, @Date,@Role); SELECT SCOPE_IDENTITY();", connection);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.Parameters.AddWithValue("@Date", user.Date);
                command.Parameters.AddWithValue("@Role", user.Role);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        // Method for user login (to retrieve user by email)
        public async Task<User> GetUserByEmail(string Email) 
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", connection);
                command.Parameters.AddWithValue("@Email", Email);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader.GetInt32("UserId"),
                            //Name = reader.GetString("Name"),
                            Name = reader.GetString("Name"),
                            Email = reader.GetString("Email"),
                            Password = reader.GetString("Password"),
                            Date = reader.GetDateTime("Date"),
                            Role = reader.GetString("Role")
                        };
                    }
                    return null;
                }
            }
        }

        public async Task<User> CreateUserFromGoogle(GoogleJsonWebSignature.Payload payload)
        {
            using (var connection = GetConnection())
            {
                // First, check if the user exists using the email from the Google payload
                var trimmedEmail = payload.Email.Trim();
                var checkUserCommand = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", connection);
                checkUserCommand.Parameters.AddWithValue("@Email", trimmedEmail);
                Console.WriteLine($"Debug - Trimmed Email: {trimmedEmail}");
                Console.WriteLine($"Debug - Parameter @Email Value: {checkUserCommand.Parameters["@Email"].Value}");
                Console.WriteLine($"Debug - Payload Email: {payload.Email}");


                await connection.OpenAsync();
                using (var reader = await checkUserCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // User found, return existing user
                        return new User
                        {
                            Id = reader.GetInt32("UserId"),
                            //Name = reader.GetString("Name"),
                            Name = reader.GetString("Name"),
                            Email = reader.GetString("Email"),
                            Password = reader.GetString("Password"),// can be null if register by google
                            Date = reader.GetDateTime("Date"),
                            Role = reader.GetString("Role")
                        };
                    }
                    else
                    {
                        // User doesn't exist, create a new user based on Google payload
                        var newUser = new User
                        {
                            Name = $"{payload.GivenName} {payload.FamilyName}",
                            Email = payload.Email,
                            Password = "", // No password for Google users
                            Date = DateTime.Now,
                            Role = "User" // Default role for new users
                        };

                        // Add new user to database
                        var userId = await AddUser(newUser);
                        newUser.Id = userId;
                        return newUser;
                    }
                }
            }
        }


        // Method to fetch all product images (no filtering by ProductId)
        public async Task<List<ProductCategory>> GetAllProductsWithImagesAsync()
        {
            var productCategories = new Dictionary<int, ProductCategory>();

            using (var connection = GetConnection())
            {
                var query = @"
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
            OR COUNT(CASE WHEN pi.ImageType = 'hover' THEN 1 END) > 0;";

                using (var command = new SqlCommand(query, connection))
                {
                    // Open the connection asynchronously
                    await connection.OpenAsync();

                    // Execute the command and process the results
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int productId = reader.GetInt32(reader.GetOrdinal("ProductId"));

                            // Check if the product already exists in the dictionary
                            if (!productCategories.TryGetValue(productId, out var productCategory))
                            {
                                productCategory = new ProductCategory
                                {
                                    ProductId = productId,
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    NewPrice = reader.GetDecimal(reader.GetOrdinal("NewPrice")),
                                    OldPrice = reader.IsDBNull(reader.GetOrdinal("OldPrice")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OldPrice")),
                                    Available = reader.GetBoolean(reader.GetOrdinal("Available")),
                                    Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Size = reader.GetString(reader.GetOrdinal("Size")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    PrimaryImages = new List<string>(),
                                    HoverImages = new List<string>()
                                };

                                productCategories.Add(productId, productCategory);
                            }

                            // Add primary image URL
                            string primaryImageUrl = reader.IsDBNull(reader.GetOrdinal("PrimaryImageUrl"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("PrimaryImageUrl"));

                            if (!string.IsNullOrEmpty(primaryImageUrl))
                            {
                                productCategory.PrimaryImages.Add(primaryImageUrl);
                            }

                            // Add hover images (parse the aggregated hover image URLs)
                            string hoverImageUrls = reader.IsDBNull(reader.GetOrdinal("HoverImageUrls"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("HoverImageUrls"));

                            if (!string.IsNullOrEmpty(hoverImageUrls))
                            {
                                // Parse the comma-separated hover image URLs into a list
                                var hoverImages = hoverImageUrls.Trim('[', ']')
                                                               .Split(',')
                                                               .Select(url => url.Trim())
                                                               .Where(url => !string.IsNullOrEmpty(url))
                                                               .ToList();

                                productCategory.HoverImages.AddRange(hoverImages);
                            }
                        }
                    }
                }
            }

            return productCategories.Values.ToList();
        }




        // New method for creating a user from Google Sign-In payload
        //public async Task<User> CreateUserFromGoogle(GoogleJsonWebSignature.Payload payload)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        // First, check if the user already exists in the database using the email from Google payload
        //        var command = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", connection);
        //        command.Parameters.AddWithValue("@Email", payload.Email);

        //        await connection.OpenAsync();
        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            if (await reader.ReadAsync())
        //            {
        //                // If the user already exists, return the existing user
        //                return new User
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    Name = reader.GetString(reader.GetOrdinal("Name")),
        //                    Email = reader.GetString(reader.GetOrdinal("Email")),
        //                    Password = reader.GetString(reader.GetOrdinal("Password")), // You might want to handle password securely
        //                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
        //                    Role = reader.GetString(reader.GetOrdinal("Role"))
        //                };
        //            }
        //            else
        //            {
        //                // If the user doesn't exist, create a new user in the database using the Google payload
        //                var newUser = new User
        //                {
        //                    Name = payload.GivenName + " " + payload.FamilyName,
        //                    Email = payload.Email,
        //                    Password = "", // Since the user is signing in with Google, no password is needed
        //                    Date = DateTime.Now,
        //                    Role = "User" // Default role for new users (you can adjust as needed)
        //                };

        //                var userId = await AddUser(newUser);
        //                newUser.Id = userId;
        //                return newUser;
        //            }
        //        }
        //    }
        //}

        // start product management
        public async Task<int> AddProduct(Product product)
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand(@"
    INSERT INTO Products (Name,Image, Category, NewPrice, OldPrice, Available, Date,Size, Description)
    VALUES (@Name,@Image, @Category, @NewPrice, @OldPrice, @Available, @Date,@Size, @Description);
    SELECT SCOPE_IDENTITY();", connection);

                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Image", product.Image);
                command.Parameters.AddWithValue("@Category", product.Category);
                command.Parameters.AddWithValue("@NewPrice", product.NewPrice);
                command.Parameters.AddWithValue("@OldPrice", product.OldPrice);
                command.Parameters.AddWithValue("@Available", product.Available);
                command.Parameters.AddWithValue("@Date", product.Date);
                command.Parameters.AddWithValue("@Size", product.Size);
                command.Parameters.AddWithValue("@Description", product.Description);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }


        // Database helper method for inserting product images                                                                                




        public async Task InsertProductImage(int productId, string imageType, string imageUrl)
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand("InsertProductImage", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@ImageType", imageType);
                command.Parameters.AddWithValue("@ImageUrl", imageUrl);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        // Method to add a product
        //public async Task<int> AddProduct(Product product)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        // Open the connection before starting the transaction
        //        await connection.OpenAsync();

        //        // Start a transaction to ensure both insertions are successful
        //        using (var transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                // First, insert the product into the Products table
        //                var command = new SqlCommand("InsertProduct", connection, transaction)
        //                {
        //                    CommandType = CommandType.StoredProcedure
        //                };

        //                command.Parameters.AddWithValue("@Name", product.Name);
        //                command.Parameters.AddWithValue("@Category", product.Category);
        //                command.Parameters.AddWithValue("@NewPrice", product.NewPrice);
        //                command.Parameters.AddWithValue("@OldPrice", product.OldPrice);
        //                command.Parameters.AddWithValue("@Available", product.Available);
        //                command.Parameters.AddWithValue("@Date", product.Date);
        //                command.Parameters.AddWithValue("@Size", product.Size);
        //                command.Parameters.AddWithValue("@Description", product.Description);

        //                var result = await command.ExecuteScalarAsync();
        //                int productId = Convert.ToInt32(result);

        //                // Now insert the primary image into ProductImages table
        //                var imageCommand = new SqlCommand("InsertProductImage", connection, transaction)
        //                {
        //                    CommandType = CommandType.StoredProcedure
        //                };

        //                imageCommand.Parameters.AddWithValue("@ProductId", productId);
        //                imageCommand.Parameters.AddWithValue("@ImageType", "primary");
        //                imageCommand.Parameters.AddWithValue("@ImageUrl", product.PrimaryImage);

        //                await imageCommand.ExecuteNonQueryAsync();

        //                // Now insert the hover image into ProductImages table
        //                imageCommand.Parameters["@ImageType"].Value = "hover";
        //                imageCommand.Parameters["@ImageUrl"].Value = product.HoverImage;

        //                await imageCommand.ExecuteNonQueryAsync();

        //                // Commit the transaction if both insertions were successful
        //                transaction.Commit();

        //                return productId;
        //            }
        //            catch (Exception ex)
        //            {
        //                // Rollback the transaction if an error occurs
        //                transaction.Rollback();
        //                throw new Exception("Error saving product and images: " + ex.Message);
        //            }
        //        }
        //    }
        //}


        public async Task<List<ProductCategory>> GetTopSoldItemsPerCategory()
        {
            var productCategories = new Dictionary<int, ProductCategory>();

            using (var connection = GetConnection())
            {
                var query = @"
    SELECT TOP 5 
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
        MAX(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END) AS HoverImageUrl,
        SUM(oi.Quantity) AS TotalSold
    FROM
        dbo.Products p
    LEFT JOIN
        dbo.ProductImages pi ON p.ProductId = pi.ProductId
    LEFT JOIN
        dbo.OrderItems oi ON p.ProductId = oi.ProductId
    LEFT JOIN
        dbo.Orders o ON oi.OrderId = o.OrderId
    WHERE
      (o.Status = 'Completed' OR o.Status IS NULL)
    GROUP BY 
        p.ProductId, p.Name, p.Category, p.NewPrice, p.OldPrice, 
        p.Available, p.Date, p.Size, p.Description
    HAVING
        MAX(CASE WHEN pi.ImageType = 'primary' THEN pi.ImageUrl END) IS NOT NULL
        OR MAX(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END) IS NOT NULL
    ORDER BY 
        TotalSold DESC;";

                using (var command = new SqlCommand(query, connection))
                {
                    

                    // Open the connection asynchronously
                    await connection.OpenAsync();

                    // Execute the command and process the results
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int productId = reader.GetInt32(reader.GetOrdinal("ProductId"));

                            // Check if the product already exists in the dictionary
                            if (!productCategories.TryGetValue(productId, out var productCategory))
                            {
                                productCategory = new ProductCategory
                                {
                                    ProductId = productId,
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    NewPrice = reader.GetDecimal(reader.GetOrdinal("NewPrice")),
                                    OldPrice = reader.IsDBNull(reader.GetOrdinal("OldPrice")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OldPrice")),
                                    Available = reader.GetBoolean(reader.GetOrdinal("Available")),
                                    Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Size = reader.GetString(reader.GetOrdinal("Size")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    PrimaryImages = new List<string>(),
                                    HoverImages = new List<string>()
                                };

                                productCategories.Add(productId, productCategory);
                            }

                            // Add image URLs to the appropriate collection
                            string primaryImageUrl = reader.IsDBNull(reader.GetOrdinal("PrimaryImageUrl"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("PrimaryImageUrl"));

                            string hoverImageUrl = reader.IsDBNull(reader.GetOrdinal("HoverImageUrl"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("HoverImageUrl"));

                            if (!string.IsNullOrEmpty(primaryImageUrl))
                            {
                                productCategory.PrimaryImages.Add(primaryImageUrl);
                            }

                            if (!string.IsNullOrEmpty(hoverImageUrl))
                            {
                                productCategory.HoverImages.Add(hoverImageUrl);
                            }
                        }
                    }
                }
            }

            return productCategories.Values.ToList();
        }

        //update product 
        public async Task<bool> UpdateProduct(Product product)
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand("UPDATE Products SET Name = @Name, Image = @Image, Category = @Category, NewPrice = @NewPrice, OldPrice = @OldPrice, Available = @Available, Date = @Date,Size=@Size, Description=@Description WHERE ProductId = @Id", connection);
                command.Parameters.AddWithValue("@Id", product.Id);
                command.Parameters.AddWithValue("@Name", product.Name);
                //command.Parameters.AddWithValue("@Image", product.Image);
                command.Parameters.AddWithValue("@Category", product.Category);
                command.Parameters.AddWithValue("@NewPrice", product.NewPrice);
                command.Parameters.AddWithValue("@OldPrice", product.OldPrice);
                command.Parameters.AddWithValue("@Available", product.Available);
                command.Parameters.AddWithValue("@Date", product.Date);
                command.Parameters.AddWithValue("@Size", product.Size);
                command.Parameters.AddWithValue("@Description", product.Description);


                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        //get all product

        public async Task<List<Product>> GetAllProducts()
        {
            var products = new List<Product>();
            using (var connection = GetConnection())
            {
                var command = new SqlCommand("SELECT ProductId, Name, Image, Category, NewPrice, OldPrice, Available, Date FROM Products Where Image IS NOT NULL", connection);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            //Image = reader.IsDBNull(2) ? null : reader.GetString(2), // Handle NULL values for Image
                            Category = reader.GetString(3),
                            NewPrice = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4), // Handle NULL values for NewPrice
                            OldPrice = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5), // Handle NULL values for OldPrice
                            Available = reader.GetBoolean(6),
                            Date = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7) // Handle NULL values for 
                        });
                    }
                }
            }
            return products;
        }

        //public async Task<List<Product>> GetAllProducts()
        //{
        //    var products = new List<Product>();
        //    using (var connection = GetConnection())
        //    {
        //        var command = new SqlCommand("GetAllProducts", connection)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        await connection.OpenAsync();
        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                products.Add(new Product
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
        //                    Name = reader.GetString(reader.GetOrdinal("Name")),
        //                    PrimaryImage = reader.IsDBNull(reader.GetOrdinal("PrimaryImage")) ? null : reader.GetString(reader.GetOrdinal("PrimaryImage")),
        //                    HoverImage = reader.IsDBNull(reader.GetOrdinal("HoverImage")) ? null : reader.GetString(reader.GetOrdinal("HoverImage")),
        //                    Category = reader.GetString(reader.GetOrdinal("Category")),
        //                    NewPrice = reader.GetDecimal(reader.GetOrdinal("NewPrice")),
        //                    OldPrice = reader.GetDecimal(reader.GetOrdinal("OldPrice")),
        //                    Available = reader.GetBoolean(reader.GetOrdinal("Available")),
        //                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
        //                    Size = reader.GetString(reader.GetOrdinal("Size")),
        //                    Description = reader.GetString(reader.GetOrdinal("Description"))
        //                });
        //            }
        //        }
        //    }
        //    return products;
        //}

        // Get Product by ID
        public async Task<Product> GetProductById(int id)
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand("SELECT ProductId, Name, Image, Category, NewPrice, OldPrice, Available, Date FROM Products WHERE ProductId = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            //Image = reader.GetString(2),
                            Category = reader.GetString(3),
                            NewPrice = reader.GetDecimal(4),
                            OldPrice = reader.GetDecimal(5),
                            Available = reader.GetBoolean(6),
                            Date = reader.GetDateTime(7)
                        };
                    }
                }
            }
            return null;
        }
        //public async Task<Product> GetProductById(int id)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        var command = new SqlCommand("GetProductById", connection)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        // Add the parameter for the product ID
        //        command.Parameters.AddWithValue("@ProductId", id);

        //        await connection.OpenAsync();
        //        using (var transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                // Associate the transaction with the command
        //                command.Transaction = transaction;

        //                using (var reader = await command.ExecuteReaderAsync())
        //                {
        //                    if (await reader.ReadAsync())
        //                    {
        //                        var product = new Product
        //                        {
        //                            Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
        //                            Name = reader.GetString(reader.GetOrdinal("Name")),
        //                            PrimaryImage = reader.IsDBNull(reader.GetOrdinal("PrimaryImage")) ? null : reader.GetString(reader.GetOrdinal("PrimaryImage")),
        //                            HoverImage = reader.IsDBNull(reader.GetOrdinal("HoverImage")) ? null : reader.GetString(reader.GetOrdinal("HoverImage")),
        //                            Category = reader.GetString(reader.GetOrdinal("Category")),
        //                            NewPrice = reader.GetDecimal(reader.GetOrdinal("NewPrice")),
        //                            OldPrice = reader.GetDecimal(reader.GetOrdinal("OldPrice")),
        //                            Available = reader.GetBoolean(reader.GetOrdinal("Available")),
        //                            Date = reader.GetDateTime(reader.GetOrdinal("Date")),
        //                            Size = reader.GetString(reader.GetOrdinal("Size")),
        //                            Description = reader.GetString(reader.GetOrdinal("Description"))
        //                        };

        //                        // Commit the transaction before returning the product
        //                        transaction.Commit();
        //                        return product;
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                // Rollback the transaction if any error occurs
        //                transaction.Rollback();
        //                throw;
        //            }
        //        }
        //    }
        //    return null;
        //}

        //get product by gategory
        public async Task<List<ProductCategory>> GetProductsByCategory(string category)
        {
            var productCategories = new Dictionary<int, ProductCategory>();

            using (var connection = GetConnection())
            {
                var query = @"
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
                p.Category = @category
            GROUP BY 
                p.ProductId, p.Name, p.Category, p.NewPrice, p.OldPrice, 
                p.Available, p.Date, p.Size, p.Description
            HAVING
                MAX(CASE WHEN pi.ImageType = 'primary' THEN pi.ImageUrl END) IS NOT NULL
                OR MAX(CASE WHEN pi.ImageType = 'hover' THEN pi.ImageUrl END) IS NOT NULL;";

                using (var command = new SqlCommand(query, connection))
                {
                    // Add the category parameter
                    command.Parameters.AddWithValue("@category", category);

                    // Open the connection asynchronously
                    await connection.OpenAsync();

                    // Execute the command and process the results
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int productId = reader.GetInt32(reader.GetOrdinal("ProductId"));

                            // Check if the product already exists in the dictionary
                            if (!productCategories.TryGetValue(productId, out var productCategory))
                            {
                                productCategory = new ProductCategory
                                {
                                    ProductId = productId,
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    NewPrice = reader.GetDecimal(reader.GetOrdinal("NewPrice")),
                                    OldPrice = reader.IsDBNull(reader.GetOrdinal("OldPrice")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OldPrice")),
                                    Available = reader.GetBoolean(reader.GetOrdinal("Available")),
                                    Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Size = reader.GetString(reader.GetOrdinal("Size")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    PrimaryImages = new List<string>(),
                                    HoverImages = new List<string>()
                                };

                                productCategories.Add(productId, productCategory);
                            }

                            // Add image URLs to the appropriate collection
                            string primaryImageUrl = reader.IsDBNull(reader.GetOrdinal("PrimaryImageUrl"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("PrimaryImageUrl"));

                            string hoverImageUrl = reader.IsDBNull(reader.GetOrdinal("HoverImageUrl"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("HoverImageUrl"));

                            if (!string.IsNullOrEmpty(primaryImageUrl))
                            {
                                productCategory.PrimaryImages.Add(primaryImageUrl);
                            }

                            if (!string.IsNullOrEmpty(hoverImageUrl))
                            {
                                productCategory.HoverImages.Add(hoverImageUrl);
                            }
                        }
                    }
                }
            }

            return productCategories.Values.ToList();
        }


        //public async Task<List<Product>> GetProductsByCategory(string category)
        //{
        //    var products = new List<Product>();

        //    // Logger setup (example using a simple console logger, replace with actual logger in production)
        //    //var logger = LogManager.GetCurrentClassLogger();

        //    try
        //    {
        //        using (var connection = GetConnection())
        //        {
        //            var command = new SqlCommand("GetProductsByCategory", connection)
        //            {
        //                CommandType = CommandType.StoredProcedure
        //            };

        //            // Add the category parameter
        //            command.Parameters.AddWithValue("@Category", category);

        //            await connection.OpenAsync();

        //            using (var transaction = connection.BeginTransaction())
        //            {
        //                try
        //                {
        //                    // Associate the transaction with the command
        //                    command.Transaction = transaction;

        //                    using (var reader = await command.ExecuteReaderAsync())
        //                    {
        //                        while (await reader.ReadAsync())
        //                        {
        //                            var product = new Product
        //                            {
        //                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
        //                                Name = reader.GetString(reader.GetOrdinal("Name")),
        //                                PrimaryImage = reader.IsDBNull(reader.GetOrdinal("PrimaryImage")) ? null : reader.GetString(reader.GetOrdinal("PrimaryImage")),
        //                                HoverImage = reader.IsDBNull(reader.GetOrdinal("HoverImage")) ? null : reader.GetString(reader.GetOrdinal("HoverImage")),
        //                                Category = reader.GetString(reader.GetOrdinal("Category")),
        //                                NewPrice = reader.GetDecimal(reader.GetOrdinal("NewPrice")),
        //                                OldPrice = reader.GetDecimal(reader.GetOrdinal("OldPrice")), 
        //                                Available = reader.GetBoolean(reader.GetOrdinal("Available")),
        //                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
        //                                Size = reader.GetString(reader.GetOrdinal("Size")),
        //                                Description = reader.GetString(reader.GetOrdinal("Description"))
        //                            };

        //                            products.Add(product);
        //                        }
        //                    }

        //                    // Commit the transaction if everything is successful
        //                    transaction.Commit();
        //                }
        //                catch (Exception ex)
        //                {
        //                    // Rollback the transaction in case of any error
        //                    transaction.Rollback();

        //                    // Log the exception
        //                    //logger.Error(ex, "Error occurred while retrieving products by category");

        //                    throw;  // Re-throw the exception
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error if connection or any other issues occur
        //        //logger.Error(ex, "Database connection or command execution failed");

        //        throw;  // Re-throw the exception for further handling (e.g., to return an error response)
        //    }

        //    return products;
        //}
        public async Task<List<Product>> GetMostRecentProducts()
        {
            var products = new List<Product>();

            using (var connection = GetConnection())
            {
                var command = new SqlCommand(@"
                SELECT ProductId, Name, Image, Category, NewPrice, OldPrice, Available, Date 
                FROM Products 
                WHERE Image IS NOT NULL AND Date <= GETDATE()
                ORDER BY Date DESC;", connection);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            //Image = reader.IsDBNull(2) ? null : reader.GetString(2), // Handle NULL values for Image
                            Category = reader.GetString(3),
                            NewPrice = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4), // Handle NULL values for NewPrice
                            OldPrice = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5), // Handle NULL values for OldPrice
                            Available = reader.GetBoolean(6),
                            Date = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7) // Handle NULL values for 
                        });
                    }
                }
            }
                



                //var command = new SqlCommand("GetMostRecentProducts", connection)
                //{
                //    CommandType = CommandType.StoredProcedure
                //};

                //await connection.OpenAsync();
                //using (var reader = await command.ExecuteReaderAsync())
                //{
                //    while (await reader.ReadAsync())
                //    {
                //        products.Add(new Product
                //        {
                //            Id = reader.GetInt32(0),
                //            Name = reader.GetString(1),
                //            Image = reader.GetString(2),
                //            Category = reader.GetString(3),
                //            NewPrice = reader.GetDecimal(4),
                //            OldPrice = reader.GetDecimal(5),
                //            Available = reader.GetBoolean(6),
                //            Date = reader.GetDateTime(7),
                //            Size = reader.GetString(8),
                //            Description = reader.GetString(9)
                //        });
                //    }
            
        

            return products;
        }

        //public async Task<List<Product>> GetMostRecentProducts()
        //{
        //    var products = new List<Product>();

        //    using (var connection = GetConnection())
        //    {
        //        await connection.OpenAsync();

        //        using (var transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                var command = new SqlCommand("GetMostRecentProducts"

        //  , connection, transaction)
        //                {
        //                    CommandType = CommandType.StoredProcedure
        //                };

        //                using (var reader = await command.ExecuteReaderAsync())
        //                {
        //                    while (await reader.ReadAsync())
        //                    {
        //                        products.Add(new Product
        //                        {
        //                            Id = reader.GetInt32(0),
        //                            Name = reader.GetString(1),
        //                            PrimaryImage = reader.IsDBNull(reader.GetOrdinal("PrimaryImage")) ? null : reader.GetString(reader.GetOrdinal("PrimaryImage")),
        //                            HoverImage = reader.IsDBNull(reader.GetOrdinal("HoverImage")) ? null : reader.GetString(reader.GetOrdinal("HoverImage")),
        //                            Category = reader.GetString(3),
        //                            NewPrice = reader.GetDecimal(4),
        //                            OldPrice = reader.GetDecimal(5),
        //                            Available = reader.GetBoolean(6),
        //                            Date = reader.GetDateTime(7),
        //                            Size = reader.GetString(8),
        //                            Description = reader.GetString(9)
        //                        });
        //                    }
        //                }

        //                // Commit transaction after successful operation
        //                await transaction.CommitAsync();
        //            }
        //            catch (Exception ex)
        //            {
        //                // Rollback the transaction in case of an error
        //                await transaction.RollbackAsync();

        //                // Log the error to the database
        //                await LogErrorToDatabase(connection, transaction, "GetMostRecentProducts", ex);

        //                // Optionally rethrow the exception or handle it further
        //                throw;
        //            }
        //        }
        //    }

        //    return products;
        //}

        //private async Task LogErrorToDatabase(SqlConnection connection, SqlTransaction transaction, string functionName, Exception exception)
        //{
        //    try
        //    {
        //        var logCommand = new SqlCommand("LogError", connection, transaction)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        logCommand.Parameters.AddWithValue("@FunctionName", functionName);
        //        logCommand.Parameters.AddWithValue("@ErrorMessage", exception.Message);
        //        logCommand.Parameters.AddWithValue("@StackTrace", exception.StackTrace);
        //        logCommand.Parameters.AddWithValue("@LogDate", DateTime.UtcNow);

        //        await logCommand.ExecuteNonQueryAsync();
        //    }
        //    catch
        //    {
        //        // Handle logging failure if necessary, e.g., log to a file or external service
        //    }
        //}


        public async Task<List<ProductImageResponse>> GetProductImagesAsync(int productId)
        {
            using (var connection = GetConnection()) // Assuming GetConnection() sets up your DB connection
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ProductId", productId, DbType.Int32);

                var images = await connection.QueryAsync<ProductImageResponse>(
                    "GetProductImagesByProductId",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return images.AsList();
            }
        }
       
        public async Task<bool> SaveCheckoutDataAsync(checkoutrequestdata request)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    // Define parameters for the stored procedure
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", request.UserId, DbType.Int32);
                    parameters.Add("@TotalAmount", request.TotalAmount, DbType.Decimal);
                    parameters.Add("@DeliveryMethod", request.DeliveryMethod, DbType.String);
                    parameters.Add("@AddressLine1", request.AddressLine1, DbType.String);
                    parameters.Add("@AddressLine2", request.AddressLine2, DbType.String);
                    parameters.Add("@City", request.City, DbType.String);
                    parameters.Add("@State", request.State, DbType.String);
                    parameters.Add("@PostalCode", request.PostalCode, DbType.String);
                    parameters.Add("@Country", request.Country, DbType.String);
                    parameters.Add("@Phone", request.Phone, DbType.String);
                    parameters.Add("@MarketingConsent", request.MarketingConsent, DbType.Boolean);


                    //decimal totalAmount = 0;

                    //foreach (var item in request.CartItems)
                    //{
                    //    // Retrieve the product details (assuming _dbHelper.GetProductById is defined)
                    //    var product = await GetProductById(item.ProductId);
                    //    if (product == null)
                    //    {
                    //        Console.WriteLine($"Product with ID {item.ProductId} not found.");
                    //        return false;
                    //    }

                    //    totalAmount += item.Quantity * product.NewPrice;

                    //    // Add CartItem details to the parameters
                    //    parameters.Add($"@ProductId_{item.ProductId}", item.ProductId, DbType.Int32);
                    //    parameters.Add($"@Quantity_{item.ProductId}", item.Quantity, DbType.Int32);
                    //    parameters.Add($"@Price_{item.ProductId}", product.NewPrice, DbType.Decimal);
                    //    parameters.Add($"@Date_{item.ProductId}", item.Date, DbType.DateTime);

                 //   var cartItems = new List<Cart>
                 // {
                 // new Cart { ProductId = 1, Quantity = 2, Price = 100.50m, Date = DateTime.Now },
                 // new Cart { ProductId = 2, Quantity = 1, Price = 50.00m, Date = DateTime.Now },
                 // new Cart { ProductId = 3, Quantity = 5, Price = 200.75m, Date = DateTime.Now }
                 //};
                 //   try
                 //   {
                 //       // Call the CreateCartItemsDataTable method
                 //       var table = CreateCartItemsDataTable(cartItems);

                 //       Console.WriteLine("DataTable created successfully:");
                 //       foreach (DataRow row in table.Rows)
                 //       {
                 //           Console.WriteLine($"ProductId: {row["ProductId"]}, Quantity: {row["Quantity"]}, Price: {row["Price"]}, Date: {row["Date"]}");
                 //       }
                 //   }
                 //   catch (Exception ex)
                 //   {
                 //       Console.WriteLine($"Error: {ex.Message}");
                 //   }

                    var cartItemsTable = CreateCartItemsDataTable(request.CartItems);
                    parameters.Add("@CartItems", cartItemsTable.AsTableValuedParameter("CartItemType"));
                

                    // Update the TotalAmount parameter with the calculated value
                    //parameters.Add("@CalculatedTotalAmount", totalAmount, DbType.Decimal);

                    // Execute the stored procedure
                    await connection.ExecuteAsync(
                        "SaveCheckoutData",
                        parameters,
                        commandType: CommandType.StoredProcedure);



                    //// Execute the stored procedure
                    //await connection.ExecuteAsync(
                    //    "SaveCheckoutData",
                    //    parameters,
                    //    commandType: CommandType.StoredProcedure);

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving checkout data: {ex.Message}");
                    return false;
                }
            }
        }



        private DataTable CreateCartItemsDataTable(List<Cart> cartItems)
        {
            // Create a new DataTable with the same structure as CartItemType
            if (cartItems == null || !cartItems.Any())
            {
                throw new ArgumentException("Cart items list is null or empty.");
            }

            //var table = new DataTable();
            //table.Columns.Add("ProductId", typeof(int));
            //table.Columns.Add("Quantity", typeof(int));
            //table.Columns.Add("Price", typeof(decimal));
            //table.Columns.Add("Date", typeof(DateTime));

            // Add rows to the DataTable based on the CartItems list
           // foreach (var item in cartItems)
           // {
           //  var row = table.NewRow();
           // //    //row["ProductId"] = item.ProductId;
           // //    //row["Quantity"] = item.Quantity;
           // //    //row["Price"] = item.Price; // Assuming Cart has Price field
           // //    //row["Date"] = item.Date;
           // //    //table.Rows.Add(row);

           // row["ProductId"] = item.ProductId;
           // row["Quantity"] = item.Quantity > 0 ? item.Quantity : throw new ArgumentException("Invalid quantity.");
           // row["Price"] = item.Price >= 0 ? item.Price : throw new ArgumentException("Invalid price.");
           // row["Date"] = item.Date != DateTime.MinValue ? item.Date : DateTime.Now;
           //Console.WriteLine($"ProductId: {row["ProductId"]}, Quantity: {row["Quantity"]}, Price: {row["Price"]}, Date: {row["Date"]}");


           //     //    table.Rows.Add(row);
           //     //    //table.Columns.Add("ProductId", typeof(int));   // Matches row["ProductId"]
           //     //    //table.Columns.Add("Quantity", typeof(int));    // Matches row["Quantity"]
           //     //    //table.Columns.Add("Price", typeof(decimal));   // Matches row["Price"]
           //     //    //table.Columns.Add("Date", typeof(DateTime));   // Matches row["Date"]



           // }

            //return ToDataTable(cartItems);
            var columnsToInclude = new List<string> { "ProductId", "Quantity", "Price", "Date" };

            // Pass only the required columns
            return ToDataTable(cartItems, columnsToInclude);
        }



        public static DataTable ToDataTable<T>(IEnumerable<T> items, IEnumerable<string> columnsToInclude)
        {
            var dataTable = new DataTable();

            // Filter properties based on the required columns
            var properties = typeof(T).GetProperties()
                .Where(p => columnsToInclude.Contains(p.Name))
                .ToList();

            // Add columns to the DataTable
            foreach (var columnName in columnsToInclude)
            {
                var prop = properties.FirstOrDefault(p => p.Name == columnName);
                if (prop != null)
                {
                    dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }

            foreach (var item in items)
            {
                //var row = dataTable.NewRow();
                DataRow row = dataTable.NewRow();
                foreach (var columnName in columnsToInclude)
                {
                    var prop = properties.FirstOrDefault(p => p.Name == columnName);
                    if (prop != null)
                    {
                        row[columnName] = prop.GetValue(item) ?? DBNull.Value;
                    }
                }
                try
                {
                    dataTable.Rows.Add(row); // Add explicitly cast row to match DataTable schema
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding row: {ex.Message}");
                }
            }

            // Add rows to the DataTable
            //foreach (var item in items)
            //{
            //    var row = dataTable.NewRow();
            //    foreach (var prop in properties)
            //    {
            //        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            //        Console.WriteLine(row[prop.Name]);

            //    }
            //    //dataTable.Rows.Add((DataRow)row);
            //    dataTable.Rows.Add(row.ItemArray);
            //}

            return dataTable;
           
        }
        public static DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            // Create columns
            foreach (var prop in typeof(T).GetProperties())
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Populate rows
            foreach (var item in items)
            {
                var row = dataTable.NewRow();
                foreach (var prop in typeof(T).GetProperties())
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }


        // Get All Products
        // Get All Products
        //public async Task<List<Product>> GetAllProducts()
        //{
        //    var products = new List<Product>();

        //    using (var connection = GetConnection())
        //    {
        //        var command = new SqlCommand("SELECT Id, Name, Image, Category, NewPrice, OldPrice, Available, Date FROM Products", connection);
        //        await connection.OpenAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                products.Add(new Product
        //                {
        //                    Id = reader.GetInt32(0),
        //                    Name = reader.GetString(1),
        //                    Image = reader.GetString(2),
        //                    Category = reader.GetString(3),
        //                    NewPrice = reader.GetDecimal(4),
        //                    OldPrice = reader.GetDecimal(5),
        //                    Available = reader.GetBoolean(6),
        //                    Date = reader.GetDateTime(7)
        //                });
        //            }
        //        }
        //    }
        //    return products;
        //}
        //order management


        // Delete Product
        public async Task<bool> DeleteProduct(int id)
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand("DELETE FROM Products WHERE ProductId = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }


        //public async Task<bool> UpdateProduct(Product product)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        var command = new SqlCommand("UPDATE Products SET Name = @Name, Image = @Image, Category = @Category, NewPrice = @NewPrice, OldPrice = @OldPrice, Available = @Available, Date = @Date,Size=@Size, Description=@Description WHERE ProductId = @Id", connection);
        //        command.Parameters.AddWithValue("@Id", product.Id);
        //        command.Parameters.AddWithValue("@Name", product.Name);
        //        command.Parameters.AddWithValue("@Image", product.Image);
        //        command.Parameters.AddWithValue("@Category", product.Category);
        //        command.Parameters.AddWithValue("@NewPrice", product.NewPrice);
        //        command.Parameters.AddWithValue("@OldPrice", product.OldPrice);
        //        command.Parameters.AddWithValue("@Available", product.Available);
        //        command.Parameters.AddWithValue("@Date", product.Date);
        //        command.Parameters.AddWithValue("@Size", product.Size);
        //        command.Parameters.AddWithValue("@Description", product.Description);


        //        await connection.OpenAsync();
        //        var rowsAffected = await command.ExecuteNonQueryAsync();
        //        return rowsAffected > 0;
        //    }
        //}

        //public async Task<bool> UpdateProduct(Product product)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        // Start a transaction to ensure both updates are handled together
        //        using (var transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                // Call the stored procedure to update product details in the Products table
        //                var productCommand = new SqlCommand("UpdateProductDetails", connection, transaction)
        //                {
        //                    CommandType = CommandType.StoredProcedure
        //                };
        //                productCommand.Parameters.AddWithValue("@ProductId", product.Id);
        //                productCommand.Parameters.AddWithValue("@Name", product.Name);
        //                productCommand.Parameters.AddWithValue("@Category", product.Category);
        //                productCommand.Parameters.AddWithValue("@NewPrice", product.NewPrice);
        //                productCommand.Parameters.AddWithValue("@OldPrice", product.OldPrice);
        //                productCommand.Parameters.AddWithValue("@Available", product.Available);
        //                productCommand.Parameters.AddWithValue("@Date", product.Date);
        //                productCommand.Parameters.AddWithValue("@Size", product.Size);
        //                productCommand.Parameters.AddWithValue("@Description", product.Description);

        //                var productRowsAffected = await productCommand.ExecuteNonQueryAsync();

        //                // Call the stored procedure to update the images
        //                var imageCommand = new SqlCommand("UpdateProductImages", connection, transaction)
        //                {
        //                    CommandType = CommandType.StoredProcedure
        //                };
        //                imageCommand.Parameters.AddWithValue("@ProductId", product.Id);
        //                imageCommand.Parameters.AddWithValue("@PrimaryImageUrl", product.PrimaryImage);
        //                imageCommand.Parameters.AddWithValue("@HoverImageUrl", product.HoverImage);

        //                await imageCommand.ExecuteNonQueryAsync();

        //                // Commit the transaction if all updates were successful
        //                transaction.Commit();

        //                return productRowsAffected > 0; // Return true if the product was updated
        //            }
        //            catch (Exception)
        //            {
        //                // Rollback the transaction if any error occurs
        //                transaction.Rollback();
        //                return false;
        //            }
        //        }
        //    }
        //}




        public async Task<bool> ProceedWithTransactionAsync(int userId, decimal totalAmount, List<OrderItem> orderItems)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert the Order
                        string insertOrderQuery = @"
                    INSERT INTO Orders (UserId, OrderDate, TotalAmount, Status)
                    VALUES (@UserId, GETDATE(), @TotalAmount, 'Completed');
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int orderId = await connection.ExecuteScalarAsync<int>(
                            insertOrderQuery,
                            new { UserId = userId, TotalAmount = totalAmount },
                            transaction);

                        // 2. Insert Order Items and Update Stock
                        foreach (var item in orderItems)
                        {
                            // Insert order item
                            string insertOrderItemQuery = @"
                        INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
                        VALUES (@OrderId, @ProductId, @Quantity, @Price);";

                            await connection.ExecuteAsync(
                                insertOrderItemQuery,
                                new
                                {
                                    OrderId = orderId,
                                    ProductId = item.ProductId,
                                    Quantity = item.Quantity,
                                    Price = item.Price
                                },
                                transaction);

                            // Update product stock
                        //string updateStockQuery = @"
                        //UPDATE Products 
                        //SET StockQuantity = StockQuantity - @Quantity 
                        //WHERE ProductId = @ProductId AND StockQuantity >= @Quantity;";

                        //    int affectedRows = await connection.ExecuteAsync(
                        //        updateStockQuery,
                        //        new { ProductId = item.ProductId, Quantity = item.Quantity },
                        //        transaction);

                        //    if (affectedRows == 0)
                        //    {
                        //        throw new Exception($"Insufficient stock for ProductId {item.ProductId}");
                        //    }
                        }

                        // 3. Clear the Cart
                        string clearCartQuery = "DELETE FROM Cart WHERE UserId = @UserId;";
                        await connection.ExecuteAsync(
                            clearCartQuery,
                            new { UserId = userId },
                            transaction);

                        // Commit the transaction
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction on error
                        transaction.Rollback();
                        Console.WriteLine($"Transaction failed: {ex.Message}");
                        return false;
                    }
                }
            }
        }


        //order management
        public async Task<int> AddOrder(int userId, decimal totalAmount)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var command = new SqlCommand(@"
                    INSERT INTO Orders (UserId, OrderDate, Status, TotalAmount)
                    VALUES (@UserId, @OrderDate, @Status, @TotalAmount);
                    SELECT SCOPE_IDENTITY();", connection);

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Status", "Pending");
                    command.Parameters.AddWithValue("@TotalAmount", totalAmount);

                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in AddOrder: {ex.Message}");
                throw; // Re-throw the exception to propagate to the controller
            }
        }


        public async Task<int> ClearCart(int userId)
        {
            using var connection = GetConnection();
            var command = new SqlCommand("DELETE FROM Cart WHERE UserId = @UserId", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }




        //public async Task<int> AddOrder(int userId, decimal totalAmount)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        var command = new SqlCommand(@"
        //    INSERT INTO Orders (UserId, OrderDate, Status,TotalAmount)
        //    VALUES (@UserId, @OrderDate,@Status, @TotalAmount);
        //    SELECT SCOPE_IDENTITY();", connection);

        //        command.Parameters.AddWithValue("@UserId", userId);
        //        command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
        //        command.Parameters.AddWithValue("@Status", "Pending");
        //        command.Parameters.AddWithValue("@TotalAmount", totalAmount);

        //        await connection.OpenAsync();
        //        var result = await command.ExecuteScalarAsync();
        //        return Convert.ToInt32(result);
        //    }
        //}

        //public async Task<int> AddOrderItem(int orderId, int productId, int quantity, decimal price)
        public async Task<int> AddOrderItem(OrderItem orderItem)
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand(@"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
            VALUES (@OrderId, @ProductId, @Quantity, @Price);", connection);

                command.Parameters.AddWithValue("@OrderId", orderItem.OrderId);
                command.Parameters.AddWithValue("@ProductId", orderItem.ProductId);
                command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
                command.Parameters.AddWithValue("@Price", orderItem.Price);

                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
        }

        //cart management
        // Cart Management
        public async Task<int> AddCartItem(Cart cartItem)
        {
            using (var connection = GetConnection())
            {
                var command = new SqlCommand(@"
                    INSERT INTO CartI (UserId, ProductId, Quantity, Date)
                    VALUES (@UserId, @ProductId, @Quantity, @Date);
                    SELECT SCOPE_IDENTITY();", connection);

                command.Parameters.AddWithValue("@UserId", cartItem.UserId);
                command.Parameters.AddWithValue("@ProductId", cartItem.ProductId);
                command.Parameters.AddWithValue("@Quantity", cartItem.Quantity);
                command.Parameters.AddWithValue("@Date", DateTime.Now);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<List<Cart>> GetCartItemsForUser(int userId)
        {
            var cartItems = new List<Cart>();
            using (var connection = GetConnection())
            {
                var command = new SqlCommand("SELECT * FROM Cart WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cartItems.Add(new Cart
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            ProductId = reader.GetInt32(2),
                            Quantity = reader.GetInt32(3),
                            Date = reader.GetDateTime(4)
                        });
                    }
                }
            }
            return cartItems;
        }
    



            // Method to get Cart Items by date range
        public async Task<List<Cart>> GetCartItemsByDateRange(DateTime startDate, DateTime endDate)
        {
            var cartItems = new List<Cart>();

            using (var connection = GetConnection())
            {
                var command = new SqlCommand(
                    "SELECT CartId, UserId, ProductId, Quantity, Date FROM Cart WHERE Date BETWEEN @StartDate AND @EndDate",
                    connection
                );

                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cartItems.Add(new Cart
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            ProductId = reader.GetInt32(2),
                            Quantity = reader.GetInt32(3),
                            Date = reader.GetDateTime(4)
                        });
                    }
                }
            }

            return cartItems;
        }






        //======================================================================



        //Method to add an item to the cart

        //public async Task<int> AddCartItem(CartItem cartItem)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        var command = new SqlCommand("INSERT INTO Cart (UserId, ProductId, Quantity) VALUES (@UserId, @ProductId, @Quantity); SELECT SCOPE_IDENTITY();", connection);
        //        command.Parameters.AddWithValue("@UserId", cartItem.UserId);
        //        command.Parameters.AddWithValue("@ProductId", cartItem.ProductId);
        //        command.Parameters.AddWithValue("@Quantity", cartItem.Quantity);

        //        await connection.OpenAsync();
        //        var result = await command.ExecuteScalarAsync();
        //        return Convert.ToInt32(result);
        //    }
        //}

        //public async Task<int> AddCartItem(CartItems cartItem)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        // Set the current system date in the format MM/DD/YYYY
        //        cartItem.Date = DateTime.Now.Date;

        //        var command = new SqlCommand(
        //            "INSERT INTO CartItems (UserId, ProductId, Quantity, Date) VALUES (@UserId, @ProductId, @Quantity, @Date); SELECT SCOPE_IDENTITY();",
        //            connection
        //        );

        //        command.Parameters.AddWithValue("@UserId", cartItem.UserId);
        //        command.Parameters.AddWithValue("@ProductId", cartItem.ProductId);
        //        command.Parameters.AddWithValue("@Quantity", cartItem.Quantity);
        //        command.Parameters.AddWithValue("@Date", cartItem.Date.ToString("MM/dd/yyyy"));

        //        await connection.OpenAsync();
        //        var result = await command.ExecuteScalarAsync();
        //        return Convert.ToInt32(result);
        //    }
        //}



        // Method to get cart items for a user
        //public async Task<List<CartItems>> GetCartItemsForUser(int userId)
        //{
        //    var cartItems = new List<CartItems>();

        //    using (var connection = GetConnection())
        //    {
        //        var command = new SqlCommand("SELECT Id, UserId, ProductId, Quantity FROM CartItems WHERE UserId = @UserId", connection);
        //        command.Parameters.AddWithValue("@UserId", userId);

        //        await connection.OpenAsync();
        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                var cartItem = new CartItems
        //                {
        //                    Id = reader.GetInt32(0),
        //                    UserId = reader.GetInt32(1),
        //                    ProductId = reader.GetInt32(2),
        //                    Quantity = reader.GetInt32(3)
        //                };
        //                cartItems.Add(cartItem);
        //            }
        //        }
        //    }

        //    return cartItems;
        //}


        // Method to get Cart Items by date range
        //public async Task<List<CartItems>> GetCartItemsByDateRange(DateTime startDate, DateTime endDate)
        //{
        //    var cartItems = new List<CartItems>();

        //    using (var connection = GetConnection())
        //    {
        //        var command = new SqlCommand("SELECT Id, UserId, ProductId, Quantity, Date FROM CartItems WHERE Date BETWEEN @StartDate AND @EndDate", connection);
        //        command.Parameters.AddWithValue("@StartDate", startDate);
        //        command.Parameters.AddWithValue("@EndDate", endDate);

        //        await connection.OpenAsync();
        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                var cartItem = new CartItems
        //                {
        //                    Id = reader.GetInt32(0),
        //                    UserId = reader.GetInt32(1),
        //                    ProductId = reader.GetInt32(2),
        //                    Quantity = reader.GetInt32(3),
        //                    // Date = reader.GetDateTime(4),
        //                    //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //                    Date = DateTime.Now


        //                };
        //                cartItems.Add(cartItem);
        //            }
        //        }
        //    }

        //    return cartItems;
        //}


        // Add this to DatabaseHelper.cs



        public async Task<List<SalesReport>> GetTotalSalesReport(DateTime startDate, DateTime endDate)
        {
            var salesReport = new List<SalesReport>();

            using (var connection = GetConnection()) // GetConnection should already be defined in your project.
            {
                var query = @"
     SELECT 
    CONVERT(VARCHAR, o.OrderDate, 103) AS [Date], -- Format as DD/MM/YYYY
    p.Category AS [Category],
    SUM(oi.Quantity) AS [TotalSalesCount]
    FROM 
    OrderItems oi
JOIN 
    Orders o ON oi.OrderId = o.OrderId
JOIN 
    Products p ON oi.ProductId = p.ProductId
WHERE 
    o.OrderDate BETWEEN @StartDate AND @EndDate -- Use parameters here
    AND o.Status = 'completed' -- Only consider completed orders
GROUP BY 
    CONVERT(VARCHAR, o.OrderDate, 103), 
    p.Category
ORDER BY 
    [Date], 
    [Category];

";

                var command = new SqlCommand(query, connection);

                // Add parameters for the query
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                // Open database connection
                await connection.OpenAsync();

                // Execute the query and process results
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        //salesReport.Add(new SalesReport
                        //{
                        //    SaleDate = reader.GetDateTime(0), // Read DateTime directly
                        //    Category = reader.GetString(1),   // Read Category as string
                        //    TotalSales = reader.GetInt32(2)   // Read TotalSales as integer
                        //});
                        salesReport.Add(new SalesReport
                        {
                            SaleDate = DateTime.ParseExact(reader.GetString(0), "dd/MM/yyyy", null), // Explicit parsing
                            Category = reader.GetString(1),
                            TotalSales = reader.GetInt32(2)
                        });

                    }
                }
            }

            return salesReport;
        }


        //public async Task<List<PopularItem>> GetTopSoldItemsPerCategory()
        //{
        //    var popularItems = new List<PopularItem>();

        //    using (var connection = GetConnection())
        //    {
        //        var query = @"
        //    SELECT TOP 5 
        //        p.ProductId,
        //        p.Name,
        //        p.Image,
        //        p.HoverImage,
        //        p.NewPrice,
        //        p.OldPrice,
        //        p.Category,
        //        p.Available,
        //        p.Size,
        //        p.Date,
        //        p.Description,
        //        SUM(oi.Quantity) AS TotalSold
        //    FROM 
        //        OrderItems oi
        //    JOIN 
        //        Products p ON oi.ProductId = p.ProductId
        //    JOIN 
        //        Orders o ON oi.OrderId = o.OrderId
        //    WHERE 
        //        o.Status = 'completed'
        //    GROUP BY 
        //        p.ProductId,
        //        p.Name, 
        //        p.Image,
        //        P.NewPrice,
        //        p.OldPrice,
        //        p.Available,
        //        p.Size,
        //        p.Date,
        //        p.Description,
        //        p.Category
        //    ORDER BY 
        //        p.Category, TotalSold DESC";

        //        var command = new SqlCommand(query, connection);

        //        await connection.OpenAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                popularItems.Add(new PopularItem
        //                {
        //                    ProductId = reader.GetInt32(0),
        //                    Name = reader.GetString(1),
        //                    Image = reader.GetString(2),
        //                    NewPrice = reader.GetDecimal(3),
        //                    OldPrice = reader.GetDecimal(4),
        //                    Category = reader.GetString(5),
        //                    Available = reader.GetBoolean(6),
        //                    Size = reader.IsDBNull(7) ? null : reader.GetString(7), // Handle null values for size
        //                    Date = reader.GetDateTime(8),
        //                    Description = reader.IsDBNull(9) ? null : reader.GetString(9), // Handle null values for description
        //                    TotalSold = reader.GetInt32(10)
        //                });
        //            }
        //        }
        //    }

        //    return popularItems;
        //}

        //public async Task<List<PopularItem>> GetTopSoldItemsPerCategory()
        //{
        //    var popularItems = new List<PopularItem>();

        //    using (var connection = GetConnection())
        //    {
        //        var query = "EXEC GetTopSoldItemsPerCategory";
        //        var command = new SqlCommand(query, connection);

        //        await connection.OpenAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                popularItems.Add(new PopularItem
        //                {
        //                    ProductId = reader.GetInt32(0),
        //                    Name = reader.GetString(1),
        //                    PrimaryImage = reader.IsDBNull(2) ? null : reader.GetString(2), // Primary image
        //                    HoverImage = reader.IsDBNull(3) ? null : reader.GetString(3),   // Hover image
        //                    NewPrice = reader.GetDecimal(4),
        //                    OldPrice = reader.GetDecimal(5),
        //                    Category = reader.GetString(6),
        //                    Available = reader.GetBoolean(7),
        //                    Size = reader.IsDBNull(8) ? null : reader.GetString(8),       // Handle null values for size
        //                    Date = reader.GetDateTime(9),
        //                    Description = reader.IsDBNull(10) ? null : reader.GetString(10), // Handle null values for description
        //                    TotalSold = reader.GetInt32(11)
        //                });
        //            }
        //        }
        //    }

        //    return popularItems;
        //}





        //        public async Task<List<SalesReport>> GetTotalSalesReport(DateTime StartDate, DateTime EndDate)
        //        {
        //            var salesReport = new List<SalesReport>();

        //            using (var connection = GetConnection())
        //            {
        //                var query = @"
        //   //         SELECT 
        //   // CONVERT(VARCHAR, o.OrderDate, 103) AS [Date], -- Format as DD/MM/YYYY
        //   // p.Category AS [Category],
        //   // SUM(oi.Quantity) AS [TotalSalesCount]
        //   //FROM 
        //   // OrderItems oi
        //   // JOIN 
        //   // Orders o ON oi.OrderId = o.OrderId
        //   // JOIN 
        //   // Products p ON oi.ProductId = p.ProductId
        //   // WHERE 
        //   // o.OrderDate BETWEEN @StartDate AND @EndDate -- Filter by date range
        //   // AND o.Status = 'completed' -- Only consider completed orders
        //   // GROUP BY 
        //   // CONVERT(VARCHAR, o.OrderDate, 103), 
        //   // p.Category
        //   // ORDER BY 
        //   // [Date], 
        //   // [Category]

        //SELECT 
        //    o.OrderDate AS [Date], -- Return DateTime
        //    p.Category AS [Category],
        //    SUM(oi.Quantity) AS [TotalSalesCount]
        //FROM 
        //    OrderItems oi
        //JOIN 
        //    Orders o ON oi.OrderId = o.OrderId
        //JOIN 
        //    Products p ON oi.ProductId = p.ProductId
        //WHERE 
        //    o.OrderDate BETWEEN @StartDate AND @EndDate -- Use parameters here
        //    AND o.Status = 'completed' -- Only consider completed orders
        //GROUP BY 
        //    o.OrderDate, 
        //    p.Category
        //ORDER BY 
        //    o.OrderDate, 
        //    p.Category;
        //;";

        //                var command = new SqlCommand(query, connection);
        //                command.Parameters.AddWithValue("@StartDate", StartDate);
        //                command.Parameters.AddWithValue("@EndDate", EndDate);

        //                await connection.OpenAsync();

        //                using (var reader = await command.ExecuteReaderAsync())
        //                {
        //                    while (await reader.ReadAsync())
        //                    {
        //                        salesReport.Add(new SalesReport
        //                        {
        //                            SaleDate = reader.GetDateTime(0),
        //                            Category = reader.GetString(1),
        //                            TotalSales = reader.GetInt32(2)
        //                        });
        //                    }
        //                }
        //            }

        //            return salesReport;
        //        }

        //public async Task<int> GetTotalSalesCount(DateTime StartDate, DateTime EndDate)
        //{
        //    int totalSales = 0;
        //    //DateTime startDate = DateTime.ParseExact(inputStartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
        //    //DateTime endDate = DateTime.ParseExact(inputEndDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);

        //    using (var connection = GetConnection())
        //    {
        //        var query = @"
        //    SELECT COUNT(*) AS TotalSales
        //    FROM Cart
        //    WHERE Cart.Date BETWEEN @StartDate AND @EndDate";

        //        var command = new SqlCommand(query, connection);
        //        command.Parameters.AddWithValue("@StartDate", StartDate);
        //        command.Parameters.AddWithValue("@EndDate", EndDate);

        //        await connection.OpenAsync();

        //        totalSales = (int)await command.ExecuteScalarAsync();
        //    }

        //    return totalSales;
        //}


        //        // Add this to DatabaseHelper.cs
        //        public async Task<List<CategorySales>> GetCategoryWiseSalesData(DateTime startDate, DateTime endDate)
        //{
        //    var salesData = new List<CategorySales>();

        //    using (var connection = GetConnection())
        //    {
        //        var query = @"
        //            SELECT Category, COUNT(*) AS TotalSales
        //            FROM CartItem
        //            INNER JOIN Products ON CartItem.ProductId = Products.Id
        //            WHERE CartItem.Date BETWEEN @StartDate AND @EndDate
        //            GROUP BY Category";

        //        var command = new SqlCommand(query, connection);
        //        command.Parameters.AddWithValue("@StartDate", startDate);
        //        command.Parameters.AddWithValue("@EndDate", endDate);

        //        await connection.OpenAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                salesData.Add(new CategorySales
        //                {
        //                    Category = reader.GetString(0),
        //                    TotalSales = reader.GetInt32(1)
        //                });
        //            }
        //        }
        //    }

        //    return salesData;
        //}



        //public async Task<List<CartItem>> GetCartItemsForUser(int userId)
        //    {
        //        var cartItems = new List<CartItem>();

        //        using (var connection = GetConnection())
        //        {
        //            var command = new SqlCommand("SELECT Id, UserId, ProductId, Quantity FROM Cart WHERE UserId = @UserId", connection);
        //            command.Parameters.AddWithValue("@UserId", userId);

        //            await connection.OpenAsync();
        //            using (var reader = await command.ExecuteReaderAsync())
        //            {
        //                while (await reader.ReadAsync())
        //                {
        //                    var cartItem = new CartItem
        //                    {
        //                        Id = reader.GetInt32(0),
        //                        UserId = reader.GetInt32(1),
        //                        ProductId = reader.GetInt32(2),
        //                        Quantity = reader.GetInt32(3)
        //                    };
        //                    cartItems.Add(cartItem);
        //                }
        //            }
        //        }

        //        return cartItems;
        //    }



        // Other methods for managing products and cart items can follow the same structure

    }
}


