using Inventory_Tracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Inventory_Tracker.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        // In Services/TokenService.cs
public string CreateToken(ApplicationUser user, IList<string> roles)
{
    var authClaims = new List<Claim>
    {
        // Use the standard NameIdentifier claim for the user's ID
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        // Use the standard Email claim
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("name", user.Name),
        new Claim("location", user.Location),
        new Claim("shopName", user.ShopName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    foreach (var role in roles)
    {
        authClaims.Add(new Claim(ClaimTypes.Role, role));
    }

    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        expires: DateTime.UtcNow.AddHours(3),
        claims: authClaims,
        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
    }
}