namespace ApiOnLamda.Model
{
    using System;
    using System.Collections.Generic;

    
        public class Orders
        {
            public int Id { get; set; }
            public int UserId { get; set; } // Foreign key to the User table
            public DateTime OrderDate { get; set; } = DateTime.Now; // When the order was placed
            public string Status { get; set; } // Status of the order (e.g., Pending, Completed, Cancelled)
            public decimal TotalAmount { get; set; } // Total cost of the order

          
        }

        // OrderItem represents the link between Order and Product/CartItem


    //public int OrderId { get; set; } // Primary Key
    //    public int UserId { get; set; } // Foreign Key to Users table
    //    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    //    public string Status { get; set; } // e.g., "Pending", "Completed"
    //    public decimal TotalAmount { get; set; }

    }

