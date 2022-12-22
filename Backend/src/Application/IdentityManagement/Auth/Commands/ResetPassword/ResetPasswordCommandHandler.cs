namespace Application.IdentityManagement.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<Unit>>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public ResetPasswordCommandHandler(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<Result<Unit>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.ResetPassword(_mapper.Map<ResetPasswordDto>(request));
    }
}
