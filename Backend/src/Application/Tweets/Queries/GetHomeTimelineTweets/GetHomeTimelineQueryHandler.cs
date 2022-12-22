using Application.Block.Shared.Interfaces;
using Application.Likes.Shared.Interfaces;
using Application.Retweets.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Application.Tweets.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Tweets.Queries.GetHomeTimelineTweets;

public class GetHomeTimelineQueryHandler : IRequestHandler<GetHomeTimelineQuery, Result<List<TweetViewModel>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHomeTimelineRepository _homeTimelineRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IRetweetRepository _retweetRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly ITweetRepository _tweetRepository;
    private readonly ILogger<GetHomeTimelineQueryHandler> _logger;

    public GetHomeTimelineQueryHandler(
        ICurrentUserService currentUserService,
        IHomeTimelineRepository homeTimelineRepository,
        IRetweetRepository retweetRepository,
        ILikeRepository likeRepository,
        IUserRepository userRepository,
        IBlockRepository blockRepository,
        ITweetRepository tweetRepository,
        ILogger<GetHomeTimelineQueryHandler> logger)
    {
        _currentUserService = currentUserService;
        _homeTimelineRepository = homeTimelineRepository;
        _retweetRepository = retweetRepository;
        _likeRepository = likeRepository;
        _userRepository = userRepository;
        _blockRepository = blockRepository;
        _tweetRepository = tweetRepository;
        _logger = logger;
    }

    public async Task<Result<List<TweetViewModel>>> Handle(GetHomeTimelineQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var homeTimelineTweet = _homeTimelineRepository.GetTweetByDescendingTime(x => x.UserId == userId, request.PageQuery.PageNumber);

        var tweetList = new List<TweetViewModel>();

        if (!homeTimelineTweet.Any())
        {
            return Result<List<TweetViewModel>>.Success(tweetList);
        }

        foreach (var homeTimeline in homeTimelineTweet)
        {
            var tweetObj = await _tweetRepository.FindByIdAsync(homeTimeline.TweetId);

            if (tweetObj == null)
            {
                continue;
            }

            var currentUserIsBlocked = await _blockRepository.FindOneByMatchAsync(x => x.BlockedId == userId && x.BlockedById == homeTimeline.TweetOwnerId);

            var tweetOwnerIsBlocked = await _blockRepository.FindOneByMatchAsync(x => x.BlockedId == homeTimeline.TweetOwnerId && x.BlockedById == userId);

            if (currentUserIsBlocked != null || tweetOwnerIsBlocked != null)
            {
                continue;
            }

            var tweetOwner = await _userRepository.FindByIdAsync(homeTimeline.TweetOwnerId);

            var tweetCreator = await _userRepository.FindByIdAsync(tweetObj.UserId!);

            if (tweetOwner == null || tweetCreator == null)
            {
                continue;
            }

            var isLikedByCurrentUser = await _likeRepository.FindOneByMatchAsync(x => x.TweetId == homeTimeline.TweetId && x.UserId == _currentUserService.UserId);

            var isRetweetedByCurrentUser = await _retweetRepository.FindOneByMatchAsync(x => x.TweetId == homeTimeline.TweetId && x.UserId == _currentUserService.UserId);

            var isRetweeted = await _retweetRepository.FindOneByMatchAsync(x => x.TweetId == homeTimeline.TweetId && x.UserId == homeTimeline.TweetOwnerId);

            var tweetVm = new TweetViewModel()
            {
                Id = tweetObj.Id,
                Content = tweetObj.Content,
                CreatedAt = tweetObj.CreatedAt,
                Likes = tweetObj.Likes,
                Retweets = tweetObj.Retweets,
                Comments = tweetObj.Comments,
                UserId = tweetOwner.Id,
                UserName = tweetOwner.Name,
                Image = tweetOwner.Image,
                TweetCreatorId = tweetCreator.Id,
                TweetCreatorName = tweetCreator.Name,
                TweetCreatorImage = tweetCreator.Image,
                CanDelete = false
            };

            if (isLikedByCurrentUser != null)
            {
                tweetVm.IsLikedByCurrentUser = true;
            }

            if (isRetweetedByCurrentUser != null)
            {
                tweetVm.IsRetweetedByCurrentUser = true;
            }

            if (isRetweeted != null)
            {
                tweetVm.IsRetweeted = true;
            }

            tweetList.Add(tweetVm);
        }

        return Result<List<TweetViewModel>>.Success(tweetList);
    }
}
