namespace Application.IdentityManagement.Auth.Commands.ForgetPassword;

public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, Result<Unit>>
{
    private readonly IIdentityService _identityService;

    public ForgetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<Unit>> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.ForgetPassword(request.Email);
    }
}
