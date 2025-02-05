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

        //var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return ServiceResult<TokenDto>.Fail("Email or Password is wrong", HttpStatusCode.NotFound);

        var result = passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);

        if (result == PasswordVerificationResult.Failed) return ServiceResult<TokenDto>.Fail("Email or Password is wrong", HttpStatusCode.NotFound);

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
}