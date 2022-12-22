namespace Application.IdentityManagement.Auth.Commands.SignIn;

public class SignInCommandHandler : IRequestHandler<SignInCommand, Result<AuthResult>>
{
    private readonly IIdentityService _identityService;

    public SignInCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthResult>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.Login(request);
    }
}
