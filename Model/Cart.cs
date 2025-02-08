namespace ApiOnLamda.Model
{
    //{
    //    public class CartItem
    //    {
    //        public int Id { get; set; }
    //        public int UserId { get; set; }
    //        public int ProductId { get; set; }
    //        public int Quantity { get; set; }
    //    }
    //}


    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; } // Make sure this property exists
       //public string Date { get; set; }
    }

}