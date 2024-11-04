using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NorebaseLikeFeature.Common.Config;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NorebaseLikeFeature.Common.Utilities
{
    public static class TokenService
    {
        public static string GenerateToken(string userId, string email, string[] roles, IOptions<AuthSettings> options)
        {
            var authSettings = options.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(ClaimTypes.Role, role)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                authSettings.Issuer,
                authSettings.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(50),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
