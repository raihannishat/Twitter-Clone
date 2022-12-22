using Application.Block.Shared.Interfaces;
using Application.Block.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Block.Commands;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Result<BlockResponse>>
{
    private readonly IBlockRepository _blockRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BlockUserCommandHandler> _logger;

    public BlockUserCommandHandler(IBlockRepository blockRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ILogger<BlockUserCommandHandler> logger)
    {
        _blockRepository = blockRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<BlockResponse>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var targetUser = await _userRepository.FindByIdAsync(request.TargetUserId);

        var currentUserId = _currentUserService.UserId;

        if (targetUser == null || request.TargetUserId == currentUserId)
        {
            return null!;
        }
        
        var blockResponse = new BlockResponse();

        var blockObj = await _blockRepository.FindOneByMatchAsync(x =>
            x.BlockedId == targetUser.Id && x.BlockedById == currentUserId);

        if (blockObj == null)
        {
            var blockEntity = new Blocks()
            {
                BlockedId = targetUser.Id,
                BlockedById = currentUserId
            };

            await _blockRepository.InsertOneAsync(blockEntity);

            blockResponse.IsBlocked = true;
        }
        else
        {
            await _blockRepository.DeleteByIdAsync(blockObj.Id);
        }

        return Result<BlockResponse>.Success(blockResponse);
    }
}
