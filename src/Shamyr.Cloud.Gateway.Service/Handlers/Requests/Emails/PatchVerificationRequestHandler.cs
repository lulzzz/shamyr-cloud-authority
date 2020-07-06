﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Shamyr.Cloud.Database.Documents;
using Shamyr.Cloud.Gateway.Service.Emails;
using Shamyr.Cloud.Gateway.Service.Repositories.Users;
using Shamyr.Cloud.Gateway.Service.Requests.Emails;
using Shamyr.Cloud.Gateway.Service.Services;

namespace Shamyr.Cloud.Gateway.Service.Handlers.Requests.Emails
{
  public class PatchVerificationRequestHandler: IRequestHandler<PatchVerificationRequest>
  {
    private readonly IUserRepository fUserRepository;
    private readonly IEmailService fEmailService;

    public PatchVerificationRequestHandler(IUserRepository userRepository, IEmailService emailService)
    {
      fUserRepository = userRepository;
      fEmailService = emailService;
    }

    public async Task<Unit> Handle(PatchVerificationRequest request, CancellationToken cancellationToken)
    {
      var user = await GetUserByEmailOrThrowAsync(request.Email, cancellationToken);

      if (user.EmailToken is null)
        throw new ConflictException($"Account with email '{request.Email}' is already verified.");

      fEmailService.SendEmailAsync(VerifyAccountEmail.New(user));

      return Unit.Value;
    }

    private async Task<UserDoc> GetUserByEmailOrThrowAsync(string email, CancellationToken cancellationToken)
    {
      var user = await fUserRepository.GetByEmailAsync(email, cancellationToken);
      if (user is null)
        throw new NotFoundException($"Account with email '{email}' does not exist.");

      return user;
    }
  }
}