namespace Application.IdentityManagement.Auth.Commands.SignUp;

public class SignUpCommandHandler : IRequestHandler<SignUpCommand, Result<Unit>>
{
    private readonly IIdentityService _identityService;

    public SignUpCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<Unit>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.Registration(request);
    }
}
