using Application.Block.Shared.Interfaces;
using Application.Likes.Shared.Interfaces;
using Application.Retweets.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Application.Tweets.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Tweets.Queries.GetUserTimelineTweets;

public class GetUserTimelineTweetsQueryHandler : IRequestHandler<GetUserTimelineTweetsQuery, Result<List<TweetViewModel>>>
{
    private readonly ITweetRepository _tweetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserTimelineRepository _userTimelineRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IRetweetRepository _retweetRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly ILogger<GetUserTimelineTweetsQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetUserTimelineTweetsQueryHandler(ITweetRepository tweetRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        IUserTimelineRepository userTimelineRepository,
        IUserRepository userRepository,
        ILikeRepository likeRepository,
        IRetweetRepository retweetRepository,
        IBlockRepository blockRepository,
        ILogger<GetUserTimelineTweetsQueryHandler> logger)
    {
        _tweetRepository = tweetRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _userTimelineRepository = userTimelineRepository;
        _userRepository = userRepository;
        _likeRepository = likeRepository;
        _retweetRepository = retweetRepository;
        _blockRepository = blockRepository;
        _logger = logger;
    }

    public async Task<Result<List<TweetViewModel>>> Handle(GetUserTimelineTweetsQuery request, CancellationToken cancellationToken)
    {
        var userId = request.PageQuery.UserId;

        var user = await _userRepository.FindByIdAsync(userId!);

        var userTimelines = _userTimelineRepository.GetTweetByDescendingTime(x => x.UserId == userId, request.PageQuery.PageNumber);

        var tweetList = new List<TweetViewModel>();

        if (!userTimelines.Any())
        {
            return Result<List<TweetViewModel>>.Success(tweetList);
        }

        var userTimelineTweet = new List<UserTimeline>();

        foreach (var userTimeline in userTimelines)
        {
            var tweet = await _tweetRepository.FindByIdAsync(userTimeline.TweetId);

            if (tweet == null)
            {
                continue;
            }

            var isCurrentUserIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == _currentUserService.UserId && x.BlockedById == tweet.UserId);

            var isTweetCreatorIsBlocked = await _blockRepository.FindOneByMatchAsync(x =>
                x.BlockedId == tweet.UserId && x.BlockedById == _currentUserService.UserId);

            if (isCurrentUserIsBlocked == null && isTweetCreatorIsBlocked == null)
            {
                userTimelineTweet.Add(userTimeline);
            }
        }

        foreach (var userTimeline in userTimelineTweet)
        {
            var tweetObj = await _tweetRepository.FindByIdAsync(userTimeline.TweetId);

            if (tweetObj == null)
            {
                continue;
            }

            var tweetCreator = await _userRepository.FindByIdAsync(tweetObj.UserId!);

            if (tweetCreator == null)
            {
                continue;
            }

            var isLikedByCurrentUser = await _likeRepository.FindOneByMatchAsync(x => x.TweetId == userTimeline.TweetId && x.UserId == _currentUserService.UserId);

            var isRetweetedByCurrentUser = await _retweetRepository.FindOneByMatchAsync(x => x.TweetId == userTimeline.TweetId && x.UserId == _currentUserService.UserId);

            var isRetweeted = await _retweetRepository.FindOneByMatchAsync(x => x.TweetId == userTimeline.TweetId && x.UserId == userId);

            var tweetVm = new TweetViewModel()
            {
                Id = tweetObj.Id,
                Content = tweetObj.Content,
                CreatedAt = tweetObj.CreatedAt,
                Likes = tweetObj.Likes,
                Retweets = tweetObj.Retweets,
                Comments = tweetObj.Comments,
                UserId = user.Id,
                UserName = user.Name,
                Image = user.Image,
                TweetCreatorId = tweetCreator.Id,
                TweetCreatorName = tweetCreator.Name,
                TweetCreatorImage = tweetCreator.Image
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

            if (tweetObj.UserId == _currentUserService.UserId)
            {
                tweetVm.CanDelete = true;
            }

            tweetList.Add(tweetVm);
        }

        return Result<List<TweetViewModel>>.Success(tweetList);
    }
}
