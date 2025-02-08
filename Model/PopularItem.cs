namespace ApiOnLamda.Model
{
    public class PopularItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        //public string PrimaryImage { get; set; } // New field for primary image
       // public string HoverImage { get; set; }   // New field for hover image
        public string Category { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public bool Available { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Size { get; set; }
        public string Description { get; set; }
        public int TotalSold { get; set; }
        public string Image { get; set; }
    }

}
