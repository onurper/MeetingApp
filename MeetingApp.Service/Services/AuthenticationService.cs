using MeetingApp.Core;
using MeetingApp.Core.DTOs;
using MeetingApp.Core.IRepositories;
using MeetingApp.Core.IServices;
using MeetingApp.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MeetingApp.Service.Services;

public class AuthenticationService(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, ITokenService tokenService, IGenericRepository<UserRefreshToken> refreshTokenRepository) : IAuthenticationService
{
    public async Task<ServiceResult<TokenDto>> CreateTokenAsync(LoginDto loginDto)
    {
        if (loginDto == null) throw new ArgumentNullException(nameof(loginDto));

        var user = await userRepository.Where(x => x.Email == loginDto.Email).FirstOrDefaultAsync();

        if (user == null) return ServiceResult<TokenDto>.Fail("Eposta veya parola hatalı", HttpStatusCode.NotFound);

        var result = passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);

        if (result == PasswordVerificationResult.Failed) return ServiceResult<TokenDto>.Fail("Eposta veya parola hatalı", HttpStatusCode.NotFound);

        var token = tokenService.CreateToken(user);

        var userRefreshToken = await refreshTokenRepository.Where(x => x.UserId == user.Id.ToString()).FirstOrDefaultAsync();

        if (userRefreshToken == null)
        {
            await refreshTokenRepository.AddAsync(new UserRefreshToken { UserId = user.Id.ToString(), Code = token.RefreshToken, Expiration = token.RefreshTokenExpiration });
        }
        else
        {
            userRefreshToken.Code = token.RefreshToken;
            userRefreshToken.Expiration = token.RefreshTokenExpiration;
        }

        await unitOfWork.SaveChangesAsync();

        return ServiceResult<TokenDto>.Success(token, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<TokenDto>> CreateTokenByRefreshToken(int userId)
    {
        var existRefreshToken = await refreshTokenRepository.Where(x => x.UserId == userId.ToString()).SingleOrDefaultAsync();

        if (existRefreshToken == null)
        {
            return ServiceResult<TokenDto>.Fail("Refresh token not found", HttpStatusCode.NotFound);
        }

        var user = await userRepository.Where(x => x.Id.ToString() == existRefreshToken.UserId).SingleOrDefaultAsync();

        if (user == null)
        {
            return ServiceResult<TokenDto>.Fail("User Id not found", HttpStatusCode.NotFound);
        }

        var tokenDto = tokenService.CreateToken(user);

        existRefreshToken.Code = tokenDto.RefreshToken;
        existRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;

        await unitOfWork.SaveChangesAsync();

        return ServiceResult<TokenDto>.Success(tokenDto, HttpStatusCode.OK);
    }
}