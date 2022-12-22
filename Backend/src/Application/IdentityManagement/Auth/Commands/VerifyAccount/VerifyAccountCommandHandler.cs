namespace Application.IdentityManagement.Auth.Commands.VerifyAccount;

public class VerifyAccountCommandHandler : IRequestHandler<VerifyAccountCommand, Result<Unit>>
{
    private readonly IIdentityService _identityService;

    public VerifyAccountCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<Unit>> Handle(VerifyAccountCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.VerifyAccount(request.Token);
    }
}
