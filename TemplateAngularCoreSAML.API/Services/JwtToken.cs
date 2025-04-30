using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TemplateAngularCoreSAML.API.Models.Dtos;
using TemplateAngularCoreSAML.API.Models.Settings;

namespace TemplateAngularCoreSAML.API.Services
{
    public class JwtToken(IConfiguration configuration) : IJwtToken
    {
        private readonly JwtSettings _jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        private readonly ConnectionFrontEnd _connectionFrontEnd = configuration.GetSection("ConnectionFrontEnd").Get<ConnectionFrontEnd>() ?? new ConnectionFrontEnd();
        public string? CreateTokenApi(UserProfileDto authTokenDto)
        {
            List<Claim> claimsApi =
            [
                new Claim("email", authTokenDto.Email),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("user", authTokenDto.PersonId),
                new Claim("payrollId", authTokenDto.PayrollId)
            ];
            return CreateToken(claimsApi);
        }

        public string TokenFrontEnd()
        {
            return _connectionFrontEnd.ClientSecret;
        }

        private string? CreateToken(List<Claim> claims)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.Now,
                Expires = DateTime.Now.Add(TimeSpan.FromMinutes(_jwtSettings.ExpireMinutes)),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = CryptographycFunctions.GetSigningCredentials(_jwtSettings.Secrets),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
