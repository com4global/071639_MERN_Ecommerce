namespace ApiOnLamda.Model
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        //public string PrimaryImage { get; set; }  // New field for primary image path
        //public string HoverImage { get; set; }    // New field for hover image path
        //public string imageType { get; set; }
        public string Category { get; set; }
        public decimal NewPrice { get; set; }  // Nullable decimal
        public decimal OldPrice { get; set; }
        public bool Available { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Size { get; set; }  // New field for size
        public string Description { get; set; }
    }

}
