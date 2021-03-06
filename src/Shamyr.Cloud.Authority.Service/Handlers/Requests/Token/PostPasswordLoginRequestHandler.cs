﻿using System.Threading;
using System.Threading.Tasks;
using Shamyr.Cloud.Authority.Service.Extensions;
using Shamyr.Cloud.Authority.Service.Repositories;
using Shamyr.Cloud.Authority.Service.Requests.Token;
using Shamyr.Cloud.Authority.Service.Services.Identity;
using Shamyr.Cloud.Database.Documents;
using Shamyr.Cloud.Services;

namespace Shamyr.Cloud.Authority.Service.Handlers.Requests.Token
{
  public class PostPasswordLoginRequestHandler: PostLoginRequestHandlerBase<PostPasswordLoginRequest>
  {
    private readonly IUserRepository fUserRepository;
    private readonly ISecretService fSecretService;

    protected override string GrantType => "password";

    public PostPasswordLoginRequestHandler(
      IUserRepository userRepository,
      ISecretService secretService,
      ITokenService tokenService,
      IUserTokenRepository userTokenRepository)
      : base(tokenService, userTokenRepository)
    {
      fUserRepository = userRepository;
      fSecretService = secretService;
    }

    protected override async Task<UserDoc> GetUserAsync(PostPasswordLoginRequest request, CancellationToken cancellationToken)
    {
      var user = await fUserRepository.GetByEmailAsync(request.Model.Email, cancellationToken);
      if (user is null)
        throw new BadRequestException($"User with email '{request.Model.Email}' not found.");
      if (user.Secret is null)
        throw new BadRequestException("User doesnt have his password set, hence cannot be connected via password login.");
      if (!fSecretService.ComparePasswords(request.Model.Password, user.Secret.ToModel()))
        throw new BadRequestException("Invalid password.");

      return user;
    }
  }
}
