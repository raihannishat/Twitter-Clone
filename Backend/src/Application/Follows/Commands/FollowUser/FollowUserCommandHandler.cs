using Application.Follows.Shared.Interfaces;
using Application.Follows.Shared.Models;
using Application.Tweets.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Commands.FollowUser;

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, Result<FollowResponse>>
{
    private readonly IFollowRepository _followRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly ITweetRepository _tweetRepository;
    private readonly IHomeTimelineRepository _homeTimelineRepository;
    private readonly ILogger<FollowUserCommandHandler> _logger;

    public FollowUserCommandHandler(IFollowRepository followRepository,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        ITweetRepository tweetRepository,
        IHomeTimelineRepository homeTimelineRepository,
        ILogger<FollowUserCommandHandler> logger)
    {
        _followRepository = followRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _tweetRepository = tweetRepository;
        _homeTimelineRepository = homeTimelineRepository;
        _logger = logger;
    }

    public async Task<Result<FollowResponse>> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var currentUser = await _userRepository.FindByIdAsync(userId);

        var targetUser = await _userRepository.FindByIdAsync(request.TargetUserId);

        if (userId == request.TargetUserId)
        {
            return null!;
        }

        if (targetUser == null)
        {
            return null!;
        }

        var followObj = await _followRepository.FindOneByMatchAsync(x =>
                x.FollowerId == userId && x.FollowedId == request.TargetUserId);

        var followResponse = new FollowResponse();

        if (followObj == null)
        {
            var followEntity = new Follow
            {
                FollowerId = userId,
                FollowedId = request.TargetUserId
            };

            await _followRepository.InsertOneAsync(followEntity);

            currentUser.Followings++;

            targetUser.Followers++;

            await _userRepository.ReplaceOneAsync(currentUser);

            await _userRepository.ReplaceOneAsync(targetUser);

            var userTweets = _tweetRepository.GetUserTweet(targetUser.Id);

            if (userTweets.Any())
            {
                foreach (var tweet in userTweets)
                {
                    var homeTimeline = new HomeTimeline
                    {
                        UserId = currentUser.Id,
                        TweetId = tweet.Id,
                        TweetOwnerId = tweet.UserId!,
                        CreatedAt = tweet.CreatedAt,
                    };

                    await _homeTimelineRepository.InsertOneAsync(homeTimeline);
                }
            }

            followResponse.IsFollowing = true;
        }
        else
        {
            await _followRepository.DeleteByIdAsync(followObj.Id);

            currentUser.Followings--;

            targetUser.Followers--;

            await _userRepository.ReplaceOneAsync(currentUser);

            await _userRepository.ReplaceOneAsync(targetUser);

            await _homeTimelineRepository.DeleteUserHomeTimeline(x =>
                x.UserId == currentUser.Id && x.TweetOwnerId == targetUser.Id);
        }

        followResponse.Followers = targetUser.Followers;

        return Result<FollowResponse>.Success(followResponse);
    }
}
