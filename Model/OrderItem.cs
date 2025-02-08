namespace ApiOnLamda.Model
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; } // Foreign key to Order table
        public int ProductId { get; set; } // Foreign key to Product table
        public int Quantity { get; set; } // Quantity of the product in the order
        public decimal Price { get; set; } // Price of the product at the time of order
                                           //    public decimal TotalPrice => Price * Quantity; // Total price for this item

        //    // Navigation properties
        //    public Orders Order { get; set; } // Reference to the parent order
        //    public Product Product { get; set; } // Reference to the product
        //}
    }
}
