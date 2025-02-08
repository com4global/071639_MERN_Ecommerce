using ApiOnLamda.Model;
using ApiOnLamda.Services;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using ApiOnLamda.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google;
using Microsoft.AspNetCore.Identity;
using log4net;
using log4net.Config;

using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace ApiOnLamda.Services
{

    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthConfig _googleAuthConfig;
        private readonly ILog _logger;
        private readonly DatabaseHelper _databaseHelper;

        public GoogleAuthService(
            IOptions<GoogleAuthConfig> googleAuthConfig,
            DatabaseHelper databaseHelper
        )
        {
            _googleAuthConfig = googleAuthConfig.Value;
            _logger = LogManager.GetLogger(typeof(GoogleAuthService));
            _databaseHelper = databaseHelper;
        }

        public async Task<BaseResponse<User>> GoogleSignIn(GoogleSignInVM model)
        {
            Payload payload = new();

            try
            {
                // Validate the Google ID token and get the payload
                payload = await ValidateAsync(model.IdToken, new ValidationSettings
                {
                    Audience = new[] { _googleAuthConfig.ClientId }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return new BaseResponse<User>(null, new List<string> { "Failed to get a response." });
            }

            // Create or get user from the database using the Google payload
            var user = await _databaseHelper.CreateUserFromGoogle(payload);

            if (user != null)
                return new BaseResponse<User>(user);
            else
                return new BaseResponse<User>(null, new List<string> { "Failed to create or fetch the user." });
        }
    }

}


