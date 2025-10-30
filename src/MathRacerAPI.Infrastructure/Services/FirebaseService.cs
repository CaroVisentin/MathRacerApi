using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace MathRacerAPI.Infrastructure.Services
{
    public class FirebaseService : Domain.Services.IFirebaseService
    {
        private static bool _initialized = false;

        public FirebaseService()
        {
            if (!_initialized)
            {
                // Detect GOOGLE_APPLICATION_CREDENTIALS or fallback to firebase-credentials.json in root
                var credPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
                if (string.IsNullOrWhiteSpace(credPath) || !System.IO.File.Exists(credPath))
                {
                    // Try relative to base directory
                    var baseDir = AppContext.BaseDirectory;
                    var fallbackPath = System.IO.Path.Combine(baseDir, "firebase-credentials.json");
                    if (System.IO.File.Exists(fallbackPath))
                    {
                        credPath = fallbackPath;
                    }
                }
                AppOptions options;
                if (!string.IsNullOrWhiteSpace(credPath) && System.IO.File.Exists(credPath))
                {
                    options = new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credPath)
                    };
                }
                else
                {
                    options = new AppOptions
                    {
                        Credential = GoogleCredential.GetApplicationDefault()
                    };
                }
                FirebaseApp.Create(options);
                _initialized = true;
            }
        }

        public async Task<string?> ValidateIdTokenAsync(string idToken)
        {
            var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            return token?.Uid;
        }
    }
}
