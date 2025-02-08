using ApiOnLamda.Model;
using ApiOnLamda.Services;

namespace ApiOnLamda.Services
{
    public interface IGoogleAuthService
    {
        Task<BaseResponse<User>> GoogleSignIn(GoogleSignInVM model);
    }
  
}
