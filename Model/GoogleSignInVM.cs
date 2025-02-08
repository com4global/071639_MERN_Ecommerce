using System.ComponentModel.DataAnnotations;

namespace ApiOnLamda.Model
{
    public class GoogleSignInVM
    {
        [Required]
        public string IdToken { get; set; }
    }
}
