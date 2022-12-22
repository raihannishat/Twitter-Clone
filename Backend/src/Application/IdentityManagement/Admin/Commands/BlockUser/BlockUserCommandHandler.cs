using Microsoft.Extensions.Logging;

namespace Application.IdentityManagement.Admin.Commands.BlockUser;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Result<AdminBlockResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BlockUserCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public BlockUserCommandHandler(IUserRepository userRepository,
        IMapper mapper,
        ILogger<BlockUserCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AdminBlockResponse>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var entity = await _userRepository.FindByIdAsync(request.Id);

        if (entity == null)
        {
            return null!;
        }

        entity.IsBlockedByAdmin = !entity.IsBlockedByAdmin;

        await _userRepository.ReplaceOneAsync(entity);

        var blockResponse = new AdminBlockResponse()
        {
            IsBlocked = entity.IsBlockedByAdmin
        };

        return Result<AdminBlockResponse>.Success(blockResponse);
    }
}
