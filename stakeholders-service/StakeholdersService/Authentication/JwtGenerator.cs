﻿using System.Security.Claims;
using System.Text;
using FluentResults;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StakeholdersService.Domain;
using StakeholdersService.Dtos;
using StakeholdersService.UseCases;

namespace StakeholdersService.Authentication
{
    public class JwtGenerator : ITokenGenerator
    {
        //private readonly string _key = Environment.GetEnvironmentVariable("JWT_KEY") ?? "explorer_secret_key";
        private readonly string _key = Environment.GetEnvironmentVariable("JWT_KEY") ?? "explorer_super_secret_key_that_is_long_enough";
        private readonly string _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "explorer";
        private readonly string _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "explorer-front.com";

        public Result<AuthenticationTokensDto> GenerateAccessToken(User user, long personId)
        {
            var authenticationResponse = new AuthenticationTokensDto();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("id", user.Id.ToString()),
                new("username", user.Username),
                new("personId", personId.ToString()),
                new(ClaimTypes.Role, user.GetPrimaryRoleName())
            };

            var jwt = CreateToken(claims, 60 * 24);
            authenticationResponse.Id = user.Id;
            authenticationResponse.AccessToken = jwt;

            return authenticationResponse;
        }

        private string CreateToken(IEnumerable<Claim> claims, double expirationTimeInMinutes)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.Now.AddMinutes(expirationTimeInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
