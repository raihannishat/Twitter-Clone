using Application.Block.Shared.Interfaces;
using Application.Follows.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries.GetFollowers;

public class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, Result<List<SearchedUserViewModel>>>
{
    private readonly IFollowRepository _followRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly ILogger<GetFollowersQueryHandler> _logger;

    public GetFollowersQueryHandler(IFollowRepository followRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IBlockRepository blockRepository,
        ILogger<GetFollowersQueryHandler> logger)
    {
        _followRepository = followRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _blockRepository = blockRepository;
        _logger = logger;
    }

    public async Task<Result<List<SearchedUserViewModel>>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        var userId = request.PageQuery.UserId;

        var targetUser = await _userRepository.FindByIdAsync(userId!);

        if (targetUser == null)
        {
            return null!;
        }

        var followerObj = _followRepository.FindByMatchWithPagination(x =>
            x.FollowedId == userId, request.PageQuery.PageNumber);

        var followerUsers = new List<Follow>();

        foreach (var follower in followerObj)
        {
            var currentUserIsBlock = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == _currentUserService.UserId && x.BlockedById == follower.FollowerId);

            var followedUserIdBlock = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == follower.FollowerId && x.BlockedById == _currentUserService.UserId);

            if (currentUserIsBlock == null && followedUserIdBlock == null)
            {
                followerUsers.Add(follower);
            }
        }

        var userCollection = _userRepository.GetCollection();

        var followers = from follower in followerUsers
                        join user in userCollection on follower.FollowerId equals user.Id
                        select new SearchedUserViewModel
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Image = user.Image
                        };

        var followerList = new List<SearchedUserViewModel>();

        if (followers.Any())
        {
            foreach (var follower in followers)
            {
                var isFollowing = await _followRepository.FindOneByMatchAsync(x =>
                    x.FollowerId == _currentUserService.UserId && x.FollowedId == follower.Id);

                if (isFollowing != null)
                {
                    follower.IsFollowing = true;
                }

                if (follower.Id == _currentUserService.UserId)
                {
                    follower.IsCurrentUser = true;
                }

                followerList.Add(follower);
            }
        }

        return Result<List<SearchedUserViewModel>>.Success(followerList.ToList());
    }
}
