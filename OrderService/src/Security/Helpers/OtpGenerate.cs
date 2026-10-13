using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace OrderService.src.Security.Helpers
{
    // <summary>
    // This class is responsible for generating and validating OTP codes.
    // It uses an in-memory cache to store the generated OTP codes along with their expiration time and the number of attempts made.
    // </summary>
    public class OtpGenerate(IMemoryCache memoryCache)
    {

     
        // private const int maxAttempts = 3; -> attempts before blocking the user
        private static readonly TimeSpan timeDuration = TimeSpan.FromMinutes(1); // duration of the code valid state
        private record OtpEntry(string Code);

        // <summary>
        // Generates a new OTP code for the given Telegram ID and stores it in the cache with an expiration time.
        // </summary>
        public string GenerateOtp(long telegramId)
        {
            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var entry = new OtpEntry(code);
            memoryCache.Set(telegramId.ToString(), entry, timeDuration);
            return code;
        }
        
        public bool ValidateOtp(long telegramId, string code)
        {
            var cacheKey = telegramId.ToString();
            if (!memoryCache.TryGetValue(cacheKey, out OtpEntry? entry) || entry == null)
            {
                return false;
            }
            var isCodeValid = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(entry.Code), Encoding.UTF8.GetBytes(code));
            if (isCodeValid)
            {
                memoryCache.Remove(cacheKey);
                return true;
            }
            return false;
        }
    }
}


