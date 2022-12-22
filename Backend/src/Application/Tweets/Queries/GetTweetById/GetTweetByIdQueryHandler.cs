using Application.Likes.Shared.Interfaces;
using Application.Retweets.Shared.Interfaces;
using Application.Tweets.Shared.Interfaces;
using Application.Tweets.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Tweets.Queries.GetTweetById;

public class GetTweetByIdQueryHandler : IRequestHandler<GetTweetByIdQuery, Result<TweetViewModel>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITweetRepository _tweetRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IRetweetRepository _retweetRepository;
    private readonly ILogger<GetTweetByIdQueryHandler> _logger;

    public GetTweetByIdQueryHandler(ICurrentUserService currentUserService,
        ITweetRepository tweetRepository,
        IUserRepository userRepository,
        ILikeRepository likeRepository,
        IRetweetRepository retweetRepository,
        ILogger<GetTweetByIdQueryHandler> logger)
    {
        _currentUserService = currentUserService;
        _tweetRepository = tweetRepository;
        _userRepository = userRepository;
        _likeRepository = likeRepository;
        _retweetRepository = retweetRepository;
        _logger = logger;
    }

    public async Task<Result<TweetViewModel>> Handle(GetTweetByIdQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var tweet = await _tweetRepository.FindByIdAsync(request.TweetId);

        if (tweet == null)
        {
            return null!;
        }

        var tweetOwner = await _userRepository.FindByIdAsync(request.TweetOwnerId);

        var tweetCreator = await _userRepository.FindByIdAsync(tweet.UserId!);

        var isLikedByCurrentUser = await _likeRepository.FindOneByMatchAsync(x =>
            x.TweetId == tweet.Id && x.UserId == currentUserId);

        var isRetweetedByCurrentUser = await _retweetRepository.FindOneByMatchAsync(x =>
            x.TweetId == tweet.Id && x.UserId == currentUserId);

        var isRetweeted = await _retweetRepository.FindOneByMatchAsync(x =>
            x.TweetId == tweet.Id && x.UserId == tweetOwner.Id);

        var tweetVm = new TweetViewModel
        {
            Id = tweet.Id,
            Content = tweet.Content,
            CreatedAt = tweet.CreatedAt,
            Likes = tweet.Likes,
            Retweets = tweet.Retweets,
            Comments = tweet.Comments,
            UserId = tweetOwner.Id,
            UserName = tweetOwner.Name,
            Image = tweetOwner.Image,
            TweetCreatorId = tweetCreator.Id,
            TweetCreatorName = tweetCreator.Name,
            TweetCreatorImage = tweetCreator.Image,
            Edited = tweet.Edited,
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

        return Result<TweetViewModel>.Success(tweetVm);
    }
}
