using Application.Block.Shared.Interfaces;
using Application.Follows.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.IdentityManagement.User.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserProfileDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFollowRepository _followRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUserRepository userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IFollowRepository followRepository,
        IBlockRepository blockRepository,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _followRepository = followRepository;
        _blockRepository = blockRepository;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(request.Id);

        var currentUser = _currentUserService.UserId;

        if (user == null)
        {
            return null!;
        }

        var userProfile = _mapper.Map<UserProfileDto>(user);

        if (userProfile.Id == _currentUserService.UserId)
        {
            userProfile.IsCurrentUserProfile = true;

            return Result<UserProfileDto>.Success(userProfile);
        }

        var isFollowing = await _followRepository.FindOneByMatchAsync(x =>
            x.FollowerId == currentUser && x.FollowedId == user.Id);

        var isBlocked = await _blockRepository.FindOneByMatchAsync(x =>
            x.BlockedById == currentUser && x.BlockedId == user.Id);

        if (isFollowing != null)
        {
            userProfile.IsFollowing = true;
        }

        if (isBlocked != null)
        {
            userProfile.IsBlockedByCurrentUser = true;
        }

        return Result<UserProfileDto>.Success(userProfile);
    }
}
