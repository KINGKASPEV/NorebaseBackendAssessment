using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NorebaseLikeFeature.Common.Config;
using NorebaseLikeFeature.Domain.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NorebaseLikeFeature.Common.Utilities
{
    public static class TokenService
    {
        public static string GenerateJwtToken(ApplicationUser user, IOptions<AuthSettings> options)
        {
            var authSettings = options.Value; 

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(authSettings.SecretKey); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Email, user.UserName)
                }),
                Expires = DateTime.UtcNow.AddDays(authSettings.TokenLifeTimeDays)
                                         .AddHours(authSettings.TokenLifeTimeInHours), 
                Issuer = authSettings.Issuer,
                Audience = authSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
