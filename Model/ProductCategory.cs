namespace ApiOnLamda.Model
{
    public class ProductCategory
    {
        // Fields from Products table
        // Fields from Products table
    public int ProductId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? OldPrice { get; set; }
        public bool Available { get; set; }
        public DateTime Date { get; set; }
        public string Size { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
        public int TotalSold { get; set; }

        // Image collections
        public List<string> PrimaryImages { get; set; } = new List<string>();
        public List<string> HoverImages { get; set; } = new List<string>();
    }

}
