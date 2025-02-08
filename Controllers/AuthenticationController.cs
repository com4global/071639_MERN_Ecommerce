using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using ApiOnLamda.Model;
using ApiOnLamda.Services;
using System.Threading.Tasks;

//namespace ApiOnLamda.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthenticationController : ControllerBase
//    {
//        private readonly IGoogleAuthService _authService;

//        public AuthenticationController(IGoogleAuthService authService)
//        {
//            _authService = authService;
//        }

//        [HttpGet("GoogleLogin")]
//        public IActionResult GoogleLogin()
//        {
//            var properties = new AuthenticationProperties { RedirectUri = "/signin-google" };
//            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
//        }

//        [HttpGet("GoogleCallback")]
//        public async Task<IActionResult> GoogleCallback()
//        {
//            var authenticateResult = await HttpContext.AuthenticateAsync();

//            if (!authenticateResult.Succeeded)
//                return BadRequest("Google authentication failed.");

//            var googleClaims = authenticateResult.Principal?.Identities
//                .FirstOrDefault()?.Claims;

//            var email = googleClaims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

//            // You can implement logic to find the user by email and sign them in
//            var user = await _authService.GoogleSignIn(new GoogleSignInVM { IdToken = email });

//            if (user != null)
//                return Ok(user);

//            return BadRequest("Failed to create user.");
//        }
//    }
//}

namespace ApiOnLamda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IGoogleAuthService _googleAuthService;

        public AuthenticationController(IGoogleAuthService googleAuthService)
        {
            _googleAuthService = googleAuthService;
        }

        [HttpPost("GoogleSignIn")]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInVM model) {
            
            if (string.IsNullOrEmpty(model?.IdToken))
            {
                return BadRequest("Invalid ID token.");
            }


            var user = await _googleAuthService.GoogleSignIn(model);
            if(user == null)
            {
            return Unauthorized("Invalid email or password.");

             }
            Console.WriteLine($"Debug - user: {user}");
            return Ok(user);
        }
    }
}
