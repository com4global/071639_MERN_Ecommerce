namespace ApiOnLamda.Model
{
    public class UploadImagesRequest
    {
        public IFormFile PrimaryImage { get; set; }
        public List<IFormFile> HoverImages { get; set; }
        public int ProductId { get; set; }
    }
}
