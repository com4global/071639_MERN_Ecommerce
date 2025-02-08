namespace ApiOnLamda.Model
{
    public class CheckoutRequest
    {
        public int UserId { get; set; }
        public List<Cart> CartItems { get; set; }
    }
}
