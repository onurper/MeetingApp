using MeetingApp.Core.DTOs;
using MeetingApp.Core.IServices;
using MeetingApp.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static MeetingApp.Core.DTOs.TokenOption;

namespace MeetingApp.Service.Services;

public class TokenService : ITokenService
{
    private readonly CustomTokenOption _tokenOption;

    public TokenService(IOptions<CustomTokenOption> options)
    {
        _tokenOption = options.Value;
    }

    public TokenDto CreateToken(User user)
    {
        var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
        var refreshTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.RefreshTokenExpiration);

        SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOption.SecurityKey)), SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
            issuer: _tokenOption.Issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.Now,
            claims: GetClaims(user, _tokenOption.Audience),
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();

        var token = handler.WriteToken(jwtSecurityToken);

        var tokenDto = new TokenDto
        {
            AccessToken = token,
            RefreshToken = CreateRefreshToken(),
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration
        };

        return tokenDto;
    }

    private IEnumerable<Claim> GetClaims(User user, List<String> audiences)
    {
        var userList = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim("ProfileImage",user.ProfileImagePath),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name,user.Name),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        };

        userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

        return userList;
    }

    private string CreateRefreshToken()

    {
        var numberByte = new Byte[32];

        using var rnd = RandomNumberGenerator.Create();

        rnd.GetBytes(numberByte);

        return Convert.ToBase64String(numberByte);
    }
}