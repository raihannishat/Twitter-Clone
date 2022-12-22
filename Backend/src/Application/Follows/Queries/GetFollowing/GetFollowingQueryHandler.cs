using Application.Block.Shared.Interfaces;
using Application.Follows.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries.GetFollowing;

public class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, Result<List<SearchedUserViewModel>>>
{
    private readonly IFollowRepository _followRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly ILogger<GetFollowingQueryHandler> _logger;

    public GetFollowingQueryHandler(IFollowRepository followRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IBlockRepository blockRepository,
        ILogger<GetFollowingQueryHandler> logger)
    {
        _followRepository = followRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _blockRepository = blockRepository;
        _logger = logger;
    }

    public async Task<Result<List<SearchedUserViewModel>>> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
    {
        var userId = request.PageQuery.UserId;

        var targetUser = await _userRepository.FindByIdAsync(userId!);

        if (targetUser == null)
        {
            return null!;
        }

        var followingObj = _followRepository.FindByMatchWithPagination(x =>
            x.FollowerId == userId, request.PageQuery.PageNumber);

        var followingsUsers = new List<Follow>();

        foreach (var following in followingObj)
        {
            var currentUserIsBlock = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == _currentUserService.UserId && x.BlockedById == following.FollowedId);

            var followedUserIdBlock = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == following.FollowedId && x.BlockedById == _currentUserService.UserId);

            if (currentUserIsBlock == null && followedUserIdBlock == null)
            {
                followingsUsers.Add(following);
            }
        }

        var userCollection = _userRepository.GetCollection();

        var followings = from followed in followingsUsers
                            join user in userCollection on followed.FollowedId equals user.Id
                            select new SearchedUserViewModel
                            {
                                Id = user.Id,
                                Name = user.Name,
                                Image = user.Image
                            };

        var followingList = new List<SearchedUserViewModel>();

        if (followings.Any())
        {
            foreach (var followed in followings)
            {
                var isFollowing = await _followRepository.FindOneByMatchAsync(x =>
                    x.FollowerId == _currentUserService.UserId && x.FollowedId == followed.Id);

                if (isFollowing != null)
                {
                    followed.IsFollowing = true;
                }

                if (followed.Id == _currentUserService.UserId)
                {
                    followed.IsCurrentUser = true;
                }

                followingList.Add(followed);
            }
        }

        return Result<List<SearchedUserViewModel>>.Success(followingList.ToList());        
    }
}
