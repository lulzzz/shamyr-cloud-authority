﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using Shamyr.Cloud.Authority.Models;
using Shamyr.Cloud.Authority.Service.Configs;
using Shamyr.Cloud.Database.Documents;
using Shamyr.Security;
using Shamyr.Security.IdentityModel;

namespace Shamyr.Cloud.Authority.Service.Services.Identity
{
  public class TokenService: ITokenService
  {
    private readonly IJwtConfig fConfig;

    public TokenService(IJwtConfig config)
    {
      fConfig = config;
    }

    public string GenerateUserJwt(UserDoc user, string grant)
    {
      var issuedAtUtc = DateTime.UtcNow;
      var claims = new List<Claim>(8)
      {
        new Claim(Constants._NameClaim, user.Id.ToString()),
        new Claim(Constants._EmailClaim, user.Email),
        new Claim(Constants._GrantClaim, grant)
      };

      if (user.Username != null)
        claims.Add(new Claim(Constants._UsernameClaim, user.Username));
      if (user.GivenName != null)
        claims.Add(new Claim(Constants._GivenNameClaim, user.GivenName));
      if (user.FamilyName != null)
        claims.Add(new Claim(Constants._FamilyNameClaim, user.FamilyName));
      if (user.Admin)
        claims.Add(new Claim(Constants._RoleClaim, nameof(UserModel.Admin)));

      using var rsa = RSA.Create();
      var dto = new JwtSecurityTokenDto
      {
        Audience = fConfig.BearerTokenAudience,
        Issuer = fConfig.BearerTokenIssuer,
        Claims = claims,
        NotBefore = issuedAtUtc,
        Expires = issuedAtUtc.AddSeconds(fConfig.BearerTokenDuration),
        SigningCredentials = rsa.ToSigningCredentials(fConfig.BearerPrivateKey)
      };

      return JwtHandler.CreateToken(dto);
    }

    public TokenDoc GenerateOrRenewRefreshToken(string? oldToken)
    {
      return new TokenDoc
      {
        Value = oldToken ?? SecurityUtils.GetUrlToken(),
        ExpirationUtc = DateTime.UtcNow.AddSeconds(fConfig.RefreshTokenDuration)
      };
    }
  }
}
